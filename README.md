### Firebase 관련 에러 발생 시 에러 처리 방법
Firebase 관련 에러 발생 시 아래 파일을 Google Drive에서 다운로드, C:\Projects\MyFirstUnitySoftware\Assets\Firebase\Plugins\x86_64 폴더에 저장하면 에러가 사라짐
- FirebaseCppApp-12_4_1.bundle
- FirebaseCppApp-12_4_1.so

### Firebase DB 및 빌드 설정
1. Database URL 주소 변경
2. 자신의 DB에 있는 google-services.json 을 Assets 폴더에 저장
3. Master / Slave에 따라 빌드설정(아래 스크립트 상단의 전처리기 변경) 변경 후 빌드
 - TCPClient.cs
 - FirebaseDBManager.cs
 - MPSManager.cs
*이미지(Master-Slave Architecture.png) 참고
이미지 설명: Master는 DB에 PLC와 센서 데이터를 전송하고 Slave는 DB에서 데이터를 받아 MPS에 적용.

### 프로그램 실행방법1(TCPClient Version)
1. PLC 프로그램을 시뮬레이션 모드로 실행
2. TCPServer 프로그램 실행
3. Unity Master 빌드 실행 후, Simulator 연결
4. Unity Slave 빌드 실행

### 프로그램 실행방법2(MxComponent Version)
1. PLC 프로그램을 시뮬레이션 모드로 실행
2. Unity Master 빌드 실행 후, Simulator 연결
3. Unity Slave 빌드 실행