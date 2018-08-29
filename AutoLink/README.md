# README

## What is This?
- 유니티 게임오브젝트와 스크립트의 SerializeField 필드의 연결을 편리하게 해줍니다.

## How to setup
~~~
using BicUtil.AutoLink;
~~~

## How to use
- AutoLink(AL)
    - 적용할 필드에 AutoLink 어트리뷰트를 추가합니다.
    ~~~
    [SerializeField] [AutoLink("TableView")]
    private TableView tableView;
    ~~~
    - 하이어라이키에서 해당 스크립트를 추가한 오브젝트를 선택하고 마우스 우클릭하면 나오는 메뉴에서 AutoLink(AL)를 선택합니다.
    - 자식 오브젝트중에서 AutoLink어트리뷰트로 설정해놓은 이름과 같은 오브젝트를 찾아 해당 필드에 자동 등록해줍니다.


- AutoLink(SF)
    - 훨씬 간편하게 사용할 수 있도록 AutoLink어트리뷰트를 사용하지 않고 SerializeField 어트리뷰트를 사용하여 만들었습니다.
    ~~~
    [SerializeField]
    private TableView shopTable;
    ~~~
    - 하이어라이키에서 해당 스크립트를 추가한 오브젝트를 선택하고 마우스 우클릭하면 나오는 메뉴에서 AutoLink(SF)를 선택합니다.
    - 자식 오브젝트 중에서 멤버이름(예제에서는 shopTable)과 같은 이름의 오브젝트를 찾아 해당 필드에 자동등록해줍니다.
    - 대소문자를 구별하지 않고 비교하여 대상을 찾아냅니다.