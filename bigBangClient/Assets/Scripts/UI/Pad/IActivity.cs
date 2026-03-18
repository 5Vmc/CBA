using GameConfig.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActivity
{
    // Start is called before the first frame update
    public void LoadActivity(ActivityData ActivityData);
}

public interface IActivityClient
{
    public void LoadActivityClient(ActivityConfig config);
}
