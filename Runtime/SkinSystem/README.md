# README #

### What is This? ###

스프라이트, UIImage, 각종 색상을 묶음으로한 스킨을 만들 수 있습니다.

SkinManager를 Active상태로 시작하는 오브젝트에 추가하고 SkinConfigList를 통해 스킨묶음에 대한 설정을 하세요.

"Resources/그룹네임/스킨네임" 폴더에 파트이름의 png파일을 추가하거나 폴더에 SkinData 파일 (마우스 우클릭, Create>Create SkinData 선택)을 생성하여 파트이름에 색상값등을 부여할 수 있습니다.

스킨을 적용할 객체에 알맞은 SkinSupportes클래스를 상속시키고 SkinManager에서 설정한 스킨그룹네임과 파트네임을 설정하면 됩니다.
