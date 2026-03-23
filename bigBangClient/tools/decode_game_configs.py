import csv
import io
import json
import re
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[1]
CONFIG_CLASS_DIR = ROOT / "Assets" / "Scripts" / "GameConfig" / "Config"
CONFIG_BYTES_DIR = ROOT / "Assets" / "LocalAsset" / "Config"
CONFIGS_CS = ROOT / "Assets" / "Scripts" / "GameConfig" / "Configs.cs"
OUTPUT_DIR = ROOT / "DecodedConfig"


LOAD_TABLE_RE = re.compile(
    r'ConfigManager\.Instance\.LoadTable<(?P<class>\w+ConfigTable)>\("(?P<table>cfg_[^"]+)"\)'
)
PROPERTY_RE = re.compile(r"public\s+(?P<type>.+?)\s+(?P<name>\w+)\s*\{\s*get;\s*private set;\s*\}")
ASSIGN_RE = re.compile(
    r"(?P<name>\w+)\s*=\s*ByteConfigReader\.(?P<reader>\w+)\(binaryReader\);"
)


class DotNetBinaryReader:
    def __init__(self, data: bytes) -> None:
        self.buffer = io.BytesIO(data)

    def read(self, size: int) -> bytes:
        chunk = self.buffer.read(size)
        if len(chunk) != size:
            raise EOFError(f"Unexpected EOF: expected {size} bytes, got {len(chunk)}")
        return chunk

    def read_int32(self) -> int:
        return int.from_bytes(self.read(4), "little", signed=True)

    def read_int64(self) -> int:
        return int.from_bytes(self.read(8), "little", signed=True)

    def read_single(self) -> float:
        import struct

        return struct.unpack("<f", self.read(4))[0]

    def read_double(self) -> float:
        import struct

        return struct.unpack("<d", self.read(8))[0]

    def read_7bit_encoded_int(self) -> int:
        value = 0
        shift = 0
        while True:
            byte = self.read(1)[0]
            value |= (byte & 0x7F) << shift
            if byte & 0x80 == 0:
                return value
            shift += 7
            if shift >= 35:
                raise ValueError("Invalid 7-bit encoded int")

    def read_string(self) -> str:
        length = self.read_7bit_encoded_int()
        return self.read(length).decode("utf-8")


def get_big_number(reader: DotNetBinaryReader) -> dict[str, Any]:
    length = reader.read_int32()
    result = {"Value": 0.0, "UnitId": 0}
    if length >= 1:
        result["Value"] = reader.read_double()
    if length >= 2:
        result["UnitId"] = reader.read_int32()
    return result


def get_int32_array(reader: DotNetBinaryReader) -> list[int]:
    return [reader.read_int32() for _ in range(reader.read_int32())]


def get_string_array(reader: DotNetBinaryReader) -> list[str]:
    return [reader.read_string() for _ in range(reader.read_int32())]


def get_float32_array(reader: DotNetBinaryReader) -> list[float]:
    return [reader.read_single() for _ in range(reader.read_int32())]


def get_int_int_dic(reader: DotNetBinaryReader) -> dict[int, int]:
    length = reader.read_int32()
    return {reader.read_int32(): reader.read_int32() for _ in range(length)}


def get_int_float_dic(reader: DotNetBinaryReader) -> dict[int, float]:
    length = reader.read_int32()
    return {reader.read_int32(): reader.read_single() for _ in range(length)}


def get_int_string_dic(reader: DotNetBinaryReader) -> dict[int, str]:
    length = reader.read_int32()
    return {reader.read_int32(): reader.read_string() for _ in range(length)}


def get_string_int_dic(reader: DotNetBinaryReader) -> dict[str, int]:
    length = reader.read_int32()
    return {reader.read_string(): reader.read_int32() for _ in range(length)}


READERS = {
    "GetBigNumber": get_big_number,
    "GetFloat32": lambda r: r.read_single(),
    "GetFloat32Array": get_float32_array,
    "GetInt32": lambda r: r.read_int32(),
    "GetInt32Array": get_int32_array,
    "GetInt64": lambda r: r.read_int64(),
    "GetIntFloatDic": get_int_float_dic,
    "GetIntIntDic": get_int_int_dic,
    "GetIntStringDic": get_int_string_dic,
    "GetString": lambda r: r.read_string(),
    "GetStringArray": get_string_array,
    "GetStringIntDic": get_string_int_dic,
}


def parse_table_mapping() -> dict[str, str]:
    mapping: dict[str, str] = {}
    text = CONFIGS_CS.read_text(encoding="utf-8")
    for match in LOAD_TABLE_RE.finditer(text):
        class_name = match.group("class").removesuffix("Table")
        mapping[class_name] = match.group("table")
    return mapping


def parse_config_schema(path: Path) -> dict[str, Any]:
    text = path.read_text(encoding="utf-8")
    properties = {m.group("name"): m.group("type").strip() for m in PROPERTY_RE.finditer(text)}
    fields = [{"name": "Id", "type": "int", "reader": "GetInt32"}]
    for match in ASSIGN_RE.finditer(text):
        name = match.group("name")
        if name == "Id":
            continue
        fields.append(
            {
                "name": name,
                "type": properties.get(name, "unknown"),
                "reader": match.group("reader"),
            }
        )
    return {"class_name": path.stem, "fields": fields}


def read_rows(schema: dict[str, Any], table_path: Path) -> tuple[list[dict[str, Any]], int]:
    data = table_path.read_bytes()
    reader = DotNetBinaryReader(data)
    row_count = reader.read_int32()
    rows: list[dict[str, Any]] = []
    for _ in range(row_count):
        row = {}
        for field in schema["fields"]:
            reader_name = field["reader"]
            row[field["name"]] = READERS[reader_name](reader)
        rows.append(row)
    trailing = len(reader.buffer.read())
    return rows, trailing


def to_csv_value(value: Any) -> str:
    if isinstance(value, (dict, list)):
        return json.dumps(value, ensure_ascii=False, separators=(",", ":"))
    if value is None:
        return ""
    return str(value)


def write_outputs(table_name: str, schema: dict[str, Any], rows: list[dict[str, Any]], trailing: int) -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    json_dir = OUTPUT_DIR / "json"
    csv_dir = OUTPUT_DIR / "csv"
    meta_dir = OUTPUT_DIR / "meta"
    json_dir.mkdir(exist_ok=True)
    csv_dir.mkdir(exist_ok=True)
    meta_dir.mkdir(exist_ok=True)

    (json_dir / f"{table_name}.json").write_text(
        json.dumps(rows, ensure_ascii=False, indent=2), encoding="utf-8"
    )

    with (csv_dir / f"{table_name}.csv").open("w", encoding="utf-8-sig", newline="") as f:
        writer = csv.writer(f)
        headers = [field["name"] for field in schema["fields"]]
        writer.writerow(headers)
        for row in rows:
            writer.writerow([to_csv_value(row.get(header)) for header in headers])

    meta = {
        "class_name": schema["class_name"],
        "table_name": table_name,
        "field_count": len(schema["fields"]),
        "row_count": len(rows),
        "trailing_bytes_after_parse": trailing,
        "fields": schema["fields"],
    }
    (meta_dir / f"{table_name}.meta.json").write_text(
        json.dumps(meta, ensure_ascii=False, indent=2), encoding="utf-8"
    )


def main() -> None:
    table_mapping = parse_table_mapping()
    manifest: list[dict[str, Any]] = []
    failures: list[dict[str, str]] = []

    for cs_path in sorted(CONFIG_CLASS_DIR.glob("*Config.cs")):
        class_name = cs_path.stem
        table_name = table_mapping.get(class_name)
        if not table_name:
            continue

        bytes_path = CONFIG_BYTES_DIR / f"{table_name}.bytes"
        if not bytes_path.exists():
            failures.append({"class_name": class_name, "table_name": table_name, "reason": "bytes file missing"})
            continue

        try:
            schema = parse_config_schema(cs_path)
            rows, trailing = read_rows(schema, bytes_path)
            write_outputs(table_name, schema, rows, trailing)
            manifest.append(
                {
                    "class_name": class_name,
                    "table_name": table_name,
                    "row_count": len(rows),
                    "field_count": len(schema["fields"]),
                    "trailing_bytes_after_parse": trailing,
                }
            )
        except Exception as ex:
            failures.append({"class_name": class_name, "table_name": table_name, "reason": str(ex)})

    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    (OUTPUT_DIR / "manifest.json").write_text(
        json.dumps({"success": manifest, "failures": failures}, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )

    readme = [
        "# DecodedConfig",
        "",
        "- `csv/`: readable table exports",
        "- `json/`: full-fidelity row exports",
        "- `meta/`: schema inferred from client config classes",
        "",
        f"- Success tables: {len(manifest)}",
        f"- Failed tables: {len(failures)}",
    ]
    (OUTPUT_DIR / "README.md").write_text("\n".join(readme), encoding="utf-8")

    print(json.dumps({"success_tables": len(manifest), "failed_tables": len(failures)}, ensure_ascii=False))


if __name__ == "__main__":
    main()
