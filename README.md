# MQTT 통신 프로그램
MQTT 통신 프로그램 구현

## MQTT란 무엇인가요?
MQTT는 머신 대 머신 통신에 사용되는 표준 기반 메시징 프로토콜 또는 규칙 세트입니다. 스마트 센서, 웨어러블 및 기타 사물 인터넷(IoT) 디바이스는 일반적으로 리소스 제약이 있는 네트워크를 통해 제한된 대역폭으로 데이터를 전송하고 수신해야 합니다. 이러한 IoT 디바이스는 MQTT를 데이터 전송에 사용하는데, 구현이 쉽고 IoT 데이터를 효율적으로 전달할 수 있기 때문입니다. MQTT는 디바이스에서 클라우드로, 클라우드에서 디바이스로의 메시징을 지원합니다.
출처 : https://aws.amazon.com/ko/what-is/mqtt/

## 실행 프로그램 화면
![image](https://github.com/HYEJJJJJ/mqtt/assets/122515375/b3f52e30-b3ed-4d8c-b64a-832353e73b86)

### 기능 구현
1. 프로그램 기본 UI 
2. 연결/해제/구독/발행
3. MQTT Form1 구현
- 버튼 클릭 시 메시지 출력
- LayoutContoller 사용하여 폼 구현
4. MQTT FrmMain 구현하기
- LayoutContoller 사용하여 폼 구현
 - MQTT클라이언트 연결
