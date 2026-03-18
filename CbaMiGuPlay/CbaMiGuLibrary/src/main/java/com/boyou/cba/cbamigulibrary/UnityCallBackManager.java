package com.boyou.cba.cbamigulibrary;

public interface UnityCallBackManager {
    public void OnLoginEnd(String userId);
    public void OnAvoidGameTrig();
    public void OnPayEnd(boolean success);
}
