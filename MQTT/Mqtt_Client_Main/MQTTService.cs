using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mqtt_Client_Main
{
    public class MQTTService
    {
        //IMqttClient: 애플리케이션이 차단 방법을 사용하여 MQTT 서버와 통신할 수 있도록 함.
        private IMqttClient _client;

        //readonly인 인터페이스 IMainView 생성
        private readonly IMainView _view;

        //enum 생성
        private EnConnectionState _connectionState;

        private bool _serviceAlive;
        private string _clientId;

        //getter
        public bool IsConnected => IsClientConnected;

        //람다식_익명함수로 IsClientConnected
        public bool IsClientConnected => _client != null && _client.IsConnected;

        //IMainView를 매개변수로 받는 MQTTService
        public MQTTService(IMainView view)
        {
            _view = view;
        }

        //Task 멈추기(Stop)
        public async Task Stop()
        {
            //_client가 null이고 _client가 연결되어 있을때
            if (_client != null && _client.IsConnected)
            {
                //await는 해당 Task가 끝날때까지 기다렸다가 완료후, await 바로 다음 실행문부터 실행 계속
                //_client 연결끊기
                await _client.DisconnectAsync();
            }
        }

        //Task 시작하기(Start) // IP, Port, Timeout, newClientId
        public async Task Start(string IP, int Port, int TimeOut, bool newClientId)
        {
            //_clientId가 null or 비어있을때 혹은 newClientId일때
            if (string.IsNullOrEmpty(_clientId) || newClientId)
                //Guid : 전역 고유 식별자 = 고유한 키 //Guid.NewGuid() 고유한 키 생성
                _clientId = string.Format("MqttClient.{0}", Guid.NewGuid());

            //MqttFactory 인스턴스 생성하여 변수 factory
            var factory = new MqttFactory();

            //MqttClentOptionsBuilder 인스턴스 생성하여 변수 options
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(IP, Port) //TcpServeer의 IP, Port와
                .WithClientId(_clientId) //_clientId를 가짐
                .Build();

            //MqttClient()의 MqttClient 생성
            _client = factory.CreateMqttClient();
            
            //ConnectedAsync 이벤트가 발생하면
            _client.ConnectedAsync += (e) =>
            {
                //수행됨
                _view?.ClientConnectionChanged();
                //Task로 반환
                return Task.CompletedTask;
            };

            //ApplicationMessageReceivedAsync 이벤트가 발생하면
            _client.ApplicationMessageReceivedAsync += (e) =>
            {
                try
                {
                    //이벤트의 ApplicationMessage을 문자열로 변환하여 message 변수에 담아줌
                    var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    //이벤트의 ApplicationMessage topic과 메세지를 받아 view에 
                    _view.MessageReceived(e.ApplicationMessage.Topic, message);
                }
                //예외 발생시 WriteDebug로 나타내줌
                catch (Exception ex)
                {
                    WriteDebug("Error UseApplicationMessageReceivedHandler - {0}", ex);
                }
                //Task로 반환
                return Task.CompletedTask; 
            };

            //DisconnectedAsync 이벤트가 발생하면
            _client.DisconnectedAsync += async (e) =>
            {
                try
                {
                    _view?.ClientConnectionChanged();

                    //_serviceAlive가 true라면
                    if (_serviceAlive)
                    {
                        Console.WriteLine("MQTT Reconnecting");

                        //시간 지연시킴
                        await Task.Delay(TimeSpan.FromSeconds(5));

                        //CancellationToken은 비동기 작업을 취소하는 용도
                        //빈 취소 토큰
                        await _client.ConnectAsync(options, CancellationToken.None);
                    }
                }
                //예외 발생시 WriteDebug로 나타내줌
                catch (Exception ex)
                {
                    WriteDebug("Error UseDisconnectedHandler = {0]", ex);
                }
            };

            //TimeOut요청시간이 초과하면
            if (TimeOut > 0)
            {
                //CancellationTokenSource 클래스는 Cancellation Token을 생성하고 Cancel 요청을 Cancellation Token들에게 보내는 일을 담당
                //TimeOut을 가지는 CancellationTokenSource 객체를 생성하여 timeout 변수에 담아줌
                using (var timeout = new CancellationTokenSource(TimeOut))
                {
                    //timeout 변수에 취소 토큰 할당
                    await _client.ConnectAsync(options, timeout.Token);
                }
            }
            else
            {
                //빈 취소 토큰
                await _client.ConnectAsync(options, CancellationToken.None);
            }
        }

        //Task Subscribe 구독 
        public async Task<bool> Subscribe(string[] topics)
        {
            //MqttTopicFilter를 타입으로 한 배열 객체 생성
            var topicFilters = new List<MQTTnet.Packets.MqttTopicFilter>();

            //매개변수 topics를 변수 topicFilters에 담는 반복문
            foreach (var item in topics)
                topicFilters.Add(new MQTTnet.Packets.MqttTopicFilter() { Topic = item });

            //MqttClienSubscribeOptions 객체 생성하고 필터 설정
            var optionSub = new MqttClientSubscribeOptions() { TopicFilters = topicFilters };

            //필터 설정하여 주제 구독
            var ret = await _client.SubscribeAsync(optionSub);

            //구독결과를 WriteDebug
            WriteDebug("Subscribe PacketIdentifier: {0}, ReasonString: {1}, Item Count: {2}, UserProperties: {3}", ret.PacketIdentifier, ret.ReasonString, ret.Items?.Count, ret.UserProperties?.Count);
            return true;
        }

        //Task Unsubscribe 구독취소
        public async Task<bool> Unsubscribe(string[] topics)
        {
            //MqttClientUnsubscribeOptions 객체 생성하고 주제 배열을 리스트로 변환하여 할당
            var optionSub = new MqttClientUnsubscribeOptions() { TopicFilters = topics.ToList() };

            //필터 설정하여 주제 구독취소
            var ret = await _client.UnsubscribeAsync(optionSub);

            //구독취소결과를 WriteDebug
            WriteDebug("UnsubscribeAsync PacketIdentifier: {0}, ReasonString: {1}, Item Count: {2}, UserProperties: {3}", ret.PacketIdentifier, ret.ReasonString, ret.Items?.Count, ret.UserProperties?.Count);
            return true;
        }

        //Task Publish 발행
        public async Task<bool> Publish(string topic, string data, int timeOut, byte qos = 0, bool retain = false)
        {
            //MqttClientPublishResult: 발행상태를  나타내는 클래스
            MqttClientPublishResult ret;

            //클라이언트가 연결되어 있지 않을 때 
            if (!IsClientConnected)
                //예외 생성
                //InvalidOperationException: 메서드 호출이 개체의 현재 상태에 대해 유효하지 않을 때 throw되는 예외
                throw new InvalidOperationException("Can't publish data - Not connected");

            //MqttApplicationMessageBuilder() 객체 생성하여
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic) //주제
                .WithPayload(data) //데이터
                .WithQualityOfServiceLevel((MQTTnet.Protocol.MqttQualityOfServiceLevel)qos) //서비스레벨의 품질
                .WithRetainFlag(retain) //메세지를 유지할지 여부
                .Build(); //생성

            //클라이언트가 연결되어 있지 않을 때 
            if (!IsClientConnected)
                WriteDebug("Not connected");

            //요청시간 초과시
            if (timeOut > 0)
            {
                //TimeOut을 가지는 CancellationTokenSource 객체를 생성하여 timeout 변수에 담아줌
                using (var timeout = new CancellationTokenSource(timeOut))
                {
                    //발행되어 메세지와 토큰을 함께 발행상태 변수인 ret에 할당
                    ret = await _client.PublishAsync(message, timeout.Token);
                    WriteDebug("Publish PacketIdentifier: {0}, ReasonString: {1}, IsSuccess: {2}, UserProperties: {3}", ret.PacketIdentifier, ret.ReasonString, ret.IsSuccess, ret.UserProperties?.Count);
                }
            }
            else
            {
                //발행 메세지와 빈 취소 토큰을 함께 변수 ret에 할당
                ret = await _client.PublishAsync(message, CancellationToken.None);
            }
            //발행상태가 성공임을 반환
            return ret.IsSuccess;
        }


        //string, params object[]를 함께 매개변수로 받는 WriteDebug 메서드
        private void WriteDebug(string format, params object[] args) 
        { 
            WriteDebug(string.Format(format, args)); 
        }

       
        //string만 매개변수로 받는 WriteDebug 메서드
        private void WriteDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }
    }
}
