### 필독1
Firebase 관련 에러 발생 시 아래 파일을 Google Drive에서 다운로드, C:\Projects\MyFirstUnitySoftware\Assets\Firebase\Plugins\x86_64 폴더에 저장하면 에러가 사라짐

FirebaseCppApp-12_4_1.bundle
FirebaseCppApp-12_4_1.so

### 필독2
Master와 Slave로 각각 빌드 시, 
TCPClient.cs / FirebaseDBManager.cs / MPSManager.cs 상단
전처리기를 MasterMode(MasterWithTCPServer or MasterWithMxComponent) or SlaveMode로 변환해야 함.
*이미지(Master-Slave Architecture.png) 참고
이미지 설명: Master는 DB에 PLC와 센서 데이터를 전송하고 Slave는 DB에서 데이터를 받아 MPS에 적용.