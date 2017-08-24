using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using k = KcpClient;
using System.Net;
using System.Linq;
using System;

public class Test : MonoBehaviour
{
    public Button ConnetBtn;
    public Button CloseBtn;
    public Button Send1Btn;
    public Button Send2Btn;
    public InputField IPPort;
    public Text Console;
    public Text Title;

    k.KcpClientEx client;
    IPEndPoint remoteipep;

    // Use this for initialization
    void Start()
    {

        ConnetBtn.onClick.AddListener(() =>
        {
            if (client != null)
            {
                client.Close();
            }
            client = new k.KcpClientEx("Test".ToCharArray().Select(a => (byte)a).ToArray(), 0, "mixpeer".ToCharArray().Select(a => (byte)a).ToArray());
            var arr = IPPort.text.Split(":"[0]);
            remoteipep = new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
            client.OnOperationResponse = (buf) =>
            {
                if (buf.Length == sizeof(UInt64))
                {
                    var i = BitConverter.ToInt64(buf, 0);
                    //Console.WriteLine($"rec unreliable:{i}");
                    Console.text += string.Format("rec unreliable:{0}\n", i);
                }
                else
                {
                    //Console.WriteLine($"rec reliable {nameof(CheckBigBBuff)}={CheckBigBBuff(buf)} size:{buf.Length} ");
                    Console.text += string.Format("rec unreliable:{0}={1} size:{2}\n", "CheckBigBBuff", Utilities.MakeTestBuff.CheckBigBBuff(buf), buf.Length);
                }
            };
            client.OnConnected = (sid) =>
            {
                this.Title.text = sid.ToString();
            };

            client.Connect(remoteipep, false);
        });
        CloseBtn.onClick.AddListener(() =>
        {
            client.Close();
            client = null;
        });
        Send1Btn.onClick.AddListener(() =>
        {
            client.SendOperationRequest(Utilities.MakeTestBuff.MakeBigBuff());
        });
        Send2Btn.onClick.AddListener(() =>
        {
            client.SendOperationRequest(BitConverter.GetBytes((UInt64)1), true);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (client != null)
        {
            client.DoWork();
            client.Service();
        }
    }
}
