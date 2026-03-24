from __future__ import annotations

from collections import defaultdict
from pathlib import Path
import sys

from scapy.all import PcapNgReader, IP, TCP


LOCAL_PREFIXES = ("192.168.", "198.18.")


def is_local(ip: str) -> bool:
    return ip.startswith(LOCAL_PREFIXES)


def main() -> int:
    if len(sys.argv) != 2:
        print("usage: python analyze-capture.py <pcapng-path>")
        return 2

    path = Path(sys.argv[1])
    if not path.exists():
        print(f"file not found: {path}")
        return 2

    flows: dict[tuple[tuple[str, int], tuple[str, int]], dict[str, object]] = defaultdict(
        lambda: {"c2s": 0, "s2c": 0, "pkts": 0, "first_payloads": [], "samples": set()}
    )

    for pkt in PcapNgReader(str(path)):
        if IP not in pkt or TCP not in pkt:
            continue

        ip = pkt[IP]
        tcp = pkt[TCP]
        payload = bytes(tcp.payload)

        if is_local(ip.src):
            client = (ip.src, tcp.sport)
            server = (ip.dst, tcp.dport)
            direction = "c2s"
        elif is_local(ip.dst):
            client = (ip.dst, tcp.dport)
            server = (ip.src, tcp.sport)
            direction = "s2c"
        else:
            continue

        row = flows[(client, server)]
        row[direction] += len(payload)
        row["pkts"] += 1

        if payload:
            samples = row["samples"]
            key = (direction, tcp.seq, len(payload), payload[:32])
            if key not in samples and len(row["first_payloads"]) < 6:
                samples.add(key)
                row["first_payloads"].append((direction, len(payload), payload[:48].hex()))

    summary = []
    for (client, server), values in flows.items():
        summary.append(
            (
                values["c2s"] + values["s2c"],
                values["pkts"],
                client,
                server,
                values["c2s"],
                values["s2c"],
                values["first_payloads"],
            )
        )

    summary.sort(reverse=True)
    for total, pkts, client, server, c2s, s2c, payloads in summary[:20]:
        print(
            f"total={total:6} pkts={pkts:4} "
            f"client={client[0]}:{client[1]} server={server[0]}:{server[1]} "
            f"c2s={c2s} s2c={s2c}"
        )
        for direction, length, payload_hex in payloads:
            print(f"  {direction} len={length} hex={payload_hex}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
