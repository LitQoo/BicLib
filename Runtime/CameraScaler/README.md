# README #

### What is This? ###

디바이스 해상도를 고려하여 설정된 표시영역이 모두 보이도록 카메라 위치를 조절합니다.

디바이스 해상도 가로 세로 비율과 설정된 표시영역 가로 세로 비율이 다를경우 상하 혹은 좌우에 검은색 레터박스가 생길 수 있습니다. 


### How to setup ###

* 카메라 오브젝트에 CameraScaler를 추가.
* MainCamera에 카메라 등록
* ReferenceTransform에 해상도레퍼런스 RectTranceform등록 혹은 ReferenceResolution에 해상도 설정.
* Vertical Align , Horizontal Align 에 화면 정렬 설정.

