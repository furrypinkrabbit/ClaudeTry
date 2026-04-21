using UnityEngine;
using UnityEngine.UI;
using GuJian.GameFlow;

public class HandleButtonClick : MonoBehaviour
{

    public Button button;

    void Start()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    // 按钮要执行的方法
    void OnButtonClicked()
    {
        GameBootstrap.Instance.StartRun();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
