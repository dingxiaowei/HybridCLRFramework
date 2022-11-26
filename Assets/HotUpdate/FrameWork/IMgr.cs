public interface IMgr
{
    void Init();
    void Start();
    void Update();
    void LateUpdate();
    void FixedUpdate();
    void OnDestroy();
    void InitOnLogin();
    void ClearOnLogout();
    void InitOnLoadScene();
}
