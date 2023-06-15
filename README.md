## 얼굴인식을 활용한 열화상 프로그램

### 개요
1. 목적 : 얼굴인식 및 실시간 체온측정을 통한 발열 환자 선별
2. 기능
   1) Opencv를  활용한  얼굴인식
   2) 열화상 카메라를 통한 체온 측정(얼굴 영역 픽셀 평균값으로 대체)
   3) 발열 환자 감지 시 알림(경보음, 아이콘)
   4) 검출된 각 인물별 데이터 저장 및 출력

### 메인 화면
![Picture2](https://github.com/kkt8282/SeokTaeJi/assets/63182249/ed199eda-1a1c-411b-b33b-d36e29e25a23)

### 정보
![Picture3](https://github.com/kkt8282/SeokTaeJi/assets/63182249/c58b1953-4eed-495a-9e03-50e10330d606)

### Requirement
* Visual Studio Nuget Package : OpenCVSharp4 & OpenCVSharp Any CPU
* haarcascade_frontalface_alt.xml
