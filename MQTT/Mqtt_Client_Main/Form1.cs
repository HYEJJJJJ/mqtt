using System;
using DevExpress.XtraBars;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Drawing.Drawing2D;

namespace Mqtt_Client_Main
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm, IMainView
    {
        //MQTTService Class로 _client 
        private MQTTService _client;

        public Form1()
        {
            InitializeComponent();

            _client = new MQTTService(this);
        }

        /// 현재 구독중인 토픽 리스트를 가져온다
        private IEnumerable<string> EnumSubscribeTopics()
        {
            foreach(var item in SubListBox.Items)
            {
                var str = item as string;
                if (!string.IsNullOrEmpty(str))
                    yield return str;
            }
        }

        //폼이 Load될 때
        protected override void OnLoad(EventArgs e)
        {
            init();

            base.OnLoad(e);
        }
        //초기값 설정
        public void init()
        {
            biHost.EditValue = "127.0.0.1";
            biPort.EditValue = "1883";
            tsCboNewClientId.Items.AddRange(new[] { "Yes", "No" });
            biCboNewClientId.EditValue = tsCboNewClientId.Items[0];
            QosComboBox.Properties.Items.AddRange(new[] {"0 At most once", "1 At least once", "2 Exactly once" });
            QosComboBox.SelectedIndex = 0;

          //  biConnectionTimeout.EditValue = "1000";
            biPublishTimeout.EditValue = "10";

            //버튼 활성화/비활성화
            SetButtonEnable();
        }

        
        //폼이 Closing될 때
        protected override async void OnClosing(CancelEventArgs e)
        {
            try
            {
                //클라이언트가 null이 아니고 연결되어있을 때
                if (_client != null && _client.IsConnected)
                {
                    //구독중인 토픽리스트에 배열 할당
                    var subTopics = EnumSubscribeTopics().ToArray();

                    //토픽의 갯수가 있다면
                    if (subTopics.Length > 0)
                        //클라이언트의 구독취소
                        await _client.Unsubscribe(subTopics);

                    //클라이언트 멈춤
                    await _client.Stop();
                }
            }
            //예외 발생시 메세지 출력
            catch (Exception ex)
            {
                MsgBox.Show(ex);
            }
            base.OnClosing(e);
        }



        //Connect 버튼 클릭 시 
        private async void tsbConnect_ItemClick(object sender, ItemClickEventArgs e)
        {
            int port;
            string ip = biHost.EditValue.ToString();
            int timeOut;

            
            //! 지양하기 ~~ == false 쓰는게 나음 
            //null 삼항연산자

            //Timeout 값이 null이거나 비어있다면 timeOut은 -1으로 빈 취소 토큰 할당
            //if (string.IsNullOrEmpty(biConnectionTimeout.EditValue.ToString()))
            //    timeOut = -1;
            //else
            //{

            //*out을 이용하면 바깥에 있는 변수에 값을 할당할 필요가 없음
            //TryParse 는 "문자열 표현을 해당하는 형으로 변환". 변환의 성공 여부 즉, true 와 false 값을 반환
            
            //    //timeOut이 유효하지 않다면 biMessage에 값 지정
            //    if (!int.TryParse(biConnectionTimeout.EditValue.ToString(), out timeOut)) 
            //    { 
            //        biMessage.EditValue = "Invalid port"; 
            //        return; 
            //    }
            //}

            string strConnTimeout = (string) biConnectionTimeout.EditValue; //ConnectionTimeout의 값을 변수에 대입

            //null이거나 비어있으면 1000을 대입하고 값이 있을 때는 strConnTimeout을 대입
            biConnectionTimeout.EditValue = string.IsNullOrEmpty(strConnTimeout) ? "1000" : strConnTimeout;

            //ConnectionTimeout의 값을 문자열에서 정수로 변환하고 그 결과값을 timeOut에 대입하는데 실패했을 때
            if (!int.TryParse(biConnectionTimeout.EditValue.ToString(), out timeOut))
            {
                biMessage.EditValue = "Invalid timeout"; //하단의 메세지 출력
                return;
            }

            //port번호가 유효하지 않다면 biMessage에 값 지정(메세지 표출)
            if (!int.TryParse(biPort.EditValue.ToString(), out port)) 
            { 
                biMessage.EditValue = "Invalid port"; 
                return; 
            }

            //Connect 버튼 비활성화
            tsbConnect.Enabled = false;

            try
            {
                //biCboNewClientId의 값과 tsCboNewClientId의 아이템 인덱스값 비교
                var isNewId = biCboNewClientId.EditValue.Equals(tsCboNewClientId.Items[0]);

                //클라이언트 시작
                await _client.Start(ip, port, timeOut, isNewId);
            }
            //예외 발생시
            catch (Exception ex)
            {
                //콘솔과 biMessage에 값 지정
                Console.WriteLine(ex);
                biMessage.EditValue = "Can't connect to server";
            }
            finally
            {
                //클라이언트가 null이 아니고 연결되어 있다면
                if (_client != null && _client.IsConnected)
                {
                    //사용자가 엔터키를 누르면 PublishButton이 눌리게 함
                    this.AcceptButton = this.PublishButton;
                    biMessage.EditValue = "";
                    MessageTextBox.EditValue = "";
                    //MessageTextBox.Clear();
                }
                //버튼 활성화/비활성화
                SetButtonEnable();
            }

        }

        //Disconnect 버튼 클릭할 때
        private async void tsbDisconnect_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Disconnect 버튼 비활성화
            tsbDisconnect.Enabled = false;
            try
            {
                //클라이언트가 null이 아니고 연결되어 있을 때
                if (_client != null && _client.IsConnected)
                {
                    //구독중인 토픽리스트에 배열 할당
                    var subTopics = EnumSubscribeTopics().ToArray();
                    //토픽의 갯수가 있다면
                    if (subTopics.Length > 0)
                        //클라이언트의 구독취소
                        await _client.Unsubscribe(subTopics);

                    //클라이언트 멈춤
                    await _client.Stop();
                }
                //SubListBox 아이템들 삭제
                SubListBox.Items.Clear();
            }
            //예외 발생시 메세지박스 표출
            catch (Exception ex)
            {
                MsgBox.Show(ex);
            }
            finally
            {
                //버튼 활성화/비활성화
                SetButtonEnable();
            }
        }

        //버튼 활성화/비활성화
        private void SetButtonEnable()
        {
            //클라이언트가 null이 아니고 연결되어 있을때 connected 변수 할당
            var connected = _client != null && _client.IsConnected;

            //연결되어 있을 때 = SubscribeButton,  PublishButton, UnsubscribeButton, Disconnect버튼 활성화
            SubscribeButton.Enabled = PublishButton.Enabled = UnsubscribeButton.Enabled = tsbDisconnect.Enabled = connected;

            //연결되어 있지 않을 때 =  Connect 버튼, Host와 Port의 TextEdit 활성화
            tsbConnect.Enabled = biHost.Enabled = biPort.Enabled = !connected;
        }

        //구독버튼을 클릭할 때
        private async void SubscribeButton_Click(object sender, EventArgs e)
        {
            //SubTopic콤보박스의 텍스트를 가져와 topic 변수
            var topic = cbSubTopic.Text.Trim();

            //topic이 null이거나 비어있다면
            if (string.IsNullOrEmpty(topic)) 
            { 
                //biMessage에 값 지정
                biMessage.EditValue = "Subscribe topic can't be empty"; 
                return; 
            }

            //구독버튼 비활성화
            SubscribeButton.Enabled = false;
            try
            {
                //클라이언트 구독
                await _client.Subscribe(new string[] { topic });

                //SubListBox에 구독 아이템 추가
                SubListBox.Items.Add(topic);
                biMessage.EditValue = "";
            }

            //MqttClientDisconnectedException 예외 발생시 메세지
            catch (MQTTnet.Client.MqttClientDisconnectedException mde)
            {
                MsgBox.Show(string.Format("Can not subscribe. {0}", mde.Message));
            }
            //예외 발생시 메세지
            catch (Exception ex)
            {
                MsgBox.Show(ex);
            }
            finally
            {
                //구독버튼 활성화
                SubscribeButton.Enabled = true;
            }
        }

        //발행버튼 클릭시
        private async void PublishButton_Click(object sender, EventArgs e)
        {
            //PubTopicTextBox의 값을 가져와 topic 변수에 담음
            var topic = PubTopicTextBox.Text.Trim();

            //topic이 null이거나 값이 비어있다면, biMessage
            if (string.IsNullOrEmpty(topic)) 
            { 
                biMessage.EditValue = "Publish topic can't be empty"; 
                return; 
            }
            //topic의 맨 처음에 #이나 +으로 시작하지 않는다면 biMessage
            if (topic.IndexOf('#') != -1 || topic.IndexOf('+') != -1) 
            { 
                biMessage.EditValue = "Publish topic can't include wildcard(#, +)"; 
            }

            //PubMessageTextBox의 값을 message 변수에 
            var message = PubMessageTextBox.Text.Trim();

            //message가 null이거나 값이 비어있다면, biMessage
            if (string.IsNullOrEmpty(message)) 
            { 
                biMessage.EditValue = "No message to send"; 
                return; 
            }
            var qos = (byte)QosComboBox.SelectedIndex;
            var retain = RetainCheckBox.Checked;

            int timeOut;

            //biPublishTimeout값이 null이거나 비어있다면 timeOut은 -1으로 빈 취소 토큰 할당
            if (string.IsNullOrEmpty(biPublishTimeout.EditValue.ToString()))
                timeOut = -1;
            else
            {
                //timeOut이 유효하지 않다면 biMessage에 값 지정
                if (!int.TryParse(biPublishTimeout.EditValue.ToString(), out timeOut)) 
                {
                    biMessage.EditValue = "Invalid Port";
                    return;
                }
            }

            //발행버튼 비활성화
            PublishButton.Enabled = false;

            try
            {
                biMessage.EditValue = "";
                //클라이언트가 발행중이라면 LogMessage
                if (await _client.Publish(topic, message, timeOut, qos, retain))
                {
                    LogMessage($">>[{DateTime.Now:HH:mm:ss}] {topic}\r\n{message}\r\n");
                }
            }
            //예외 발생시 메세지박스
            catch (Exception ex)
            {
                MsgBox.Show(ex);
            }
            finally
            {
                //발행버튼 활성화
                PublishButton.Enabled = true;
            }
        }

        //Clear 버튼 클릭시 메시지박스 초기화
        private void ClearButton_Click(object sender, EventArgs e)
        {
            MessageTextBox.EditValue = "";
            //MessageTextBox.Clear();
        }

        //구독취소버튼 클릭시
        private async void UnsubscribeButton_Click(object sender, EventArgs e)
        {
            //SubListBox의 아이템들 topic에 담음//string값이면 string 반환
            var topic = SubListBox.SelectedItem as string;

            //topic이 null이거나 비어있다면 biMessage
            if (string.IsNullOrEmpty(topic))
            {
                biMessage.EditValue = "Select topic to unscribe";
                return;
            }
            try
            {
                biMessage.EditValue = "";
                //클라이언트가 구독취소했다면 subListBox 아이템들 삭제
                await _client.Unsubscribe(new string[] { topic });
                SubListBox.Items.Remove(SubListBox.SelectedItem);
            }
            //예외 발생시 메세지박스
            catch (Exception ex)
            {
                MsgBox.Show(ex);
            }
        }

        //클라이언트 연결 바뀔시의 메서드
        public void ClientConnectionChanged()
        {
            if (this.InvokeRequired)
                this.Invoke(new Action(SetButtonEnable));
            else
                //버튼 활성화/비활성화
                SetButtonEnable();
        }

        //메세지를 받을때의 메서드 
        public void MessageReceived(string topic, string data)
        {
            LogMessage($"<<[{DateTime.Now:HH:mm:ss}] {topic}\r\n{data}\r\n");
        }

        //LogMessage 메서드
        private void LogMessage(string myStr)
        {
            if (MessageTextBox.InvokeRequired)
                MessageTextBox.Invoke(new Action<string>(LogMessage), myStr);
            else
                MessageTextBox.MaskBox.AppendText(myStr);
        }


        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.Item is BarButtonItem)
            {
                var button = e.Item as BarButtonItem;

                if (button.Name == "tsbConnect")
                {

                }

            }

        }
    }
}
