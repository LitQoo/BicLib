# README

## What is This?
- 유니티 게임오브젝트와 스크립트의 SerializeField 필드의 연결을 편리하게 해줍니다.

## How to setup
~~~
using BicUtil.AutoLink;
~~~

## How to use
- 적용할 필드에 AutoLink 어트리뷰트를 추가합니다.
~~~
[SerializeField] [AutoLink("TableView")]
private TableView tableView;
~~~
- 하이어라이키에서 해당 스크립트를 추가한 오브젝트를 선택하고 마우스 우클릭하면 나오는 메뉴에서 AutoLink를 선택합니다.
- 자식 오브젝트중에서 AutoLink어트리뷰트로 설정해놓은 이름과 같은 오브젝트를 찾아 해당 필드에 자동 등록해줍니다.