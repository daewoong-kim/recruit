# ZIGBANG ZED Assignment

안녕하세요. 직방 ZED 팀 지원자 여러분. 지원해주셔서 감사합니다.

과제 설명 전 간단한 안내 말씀 드리겠습니다.

직방 ZED 팀은 유니티 엔진을 이용하여 직방에 있는 부동산 빅데이터들을

3D 정보로 시각화해주는 일을 하고 있습니다.

이와 관련하여 과제도 간단한 데이터를 이용해 3D 시각화로 진행하고 있습니다.

Github 의 fork 기능을 이용하여 개인 저장소로 복사해주신 다음 

코드를 작성 및 README.MD 파일 수정 후 개인 저장소의 URL을 채용 담당자에게 공유해주시면 됩니다.

최하단의 [문제 해결 방법] 섹션은 면접에 도움이 될 수 있도록

과제를 하면서 고민했던 부분, 해결방안 등에 대해 자유롭게 작성해주시면 됩니다.

## 과제 설명

- 과제는 유니티 2019.4.21f1 으로 작성합니다.

- Scenes/ZedScene.unity 씬에서 확인 가능하도록 작성부탁드립니다.

### 문제1) 단지에 있는 동의 폴리곤 데이터를 이용하여 런타임에 메쉬를 구성하여 렌더링해주세요.(40점)

샘플 데이터는 아래의 경로에 위치해 있습니다.

Assets/Samples/json/dong.json

다음과 같이 구성되어 있습니다.

```
API Response 구성
{
    success: boolean, // API 성공 여부
    code: number, // API 결과 코드
    data: [] // 동 데이터 배열
}

동 데이터 구성
{
    roomtypes: [], // 타입 데이터의 배열
    meta: {
        bd_id: number, // 동 id
        동: string, // 동 이름
        지면높이: number, // 지면으로 부터의 높이
    } // 메타 정보
}

타입 데이터 구성
{
    coordinatesBase64s: string[], // 폴리곤 데이터의 배열
    meta: {
        룸타입id: number // 타입 id
    }
}

폴리곤 데이터의 구성 (*중요*)
- float[] 을 byte[] 로 변환후 Base64 로 인코딩된 데이터
- float[] 는 3개씩 나누어 Vector3 의 x,z,y 에 매핑됩니다.
  ex) [1.0, 2.0, 3.0, 4.0, 5.0, 6.0]
  -> [Vector3(1.0, 3.0, 2.0), Vector3(4.0, 6.0, 5.0)]
- C# 의 Buffer.BlockCopy, Convert.FromBase64String 등의 함수를 이용하시면 됩니다.
```
**폴리곤 데이터 1당 유니티 좌표계의 1m 를 의미합니다.**

아래의 그림과 유사한 단지 구성이 되어야 합니다. 평형별 색상은 고려하지 않으셔도 됩니다.

![결과1](https://i.imgur.com/vcmmiid.png)



### 문제2) 문제1의 결과를 바탕으로 생성된 메쉬에 텍스쳐를 매핑합니다.(60점)

텍스쳐의 위치는 아래와 같습니다.

Assets/Samples/texture/buildingTester_d.png

텍스쳐는 아래와 같이 구성되어 있습니다.
![Texture](https://i.imgur.com/srIPHBq.png)


텍스쳐 매핑은 다음과 같습니다.

1번(앞) : 정점의 normal과 Vector3.forward의 각도가 y축을 기준으로 180 <= n && n <= 220 에 해당하는 경우

2번 : 1, 3을 제외한 모든 경우

3번(위, 아래) : 정점의 normal이 y축을 기준으로 Vector3.up || Vector3.down과 일치하는 경우


텍스쳐 파일을 자르지 않고 한장으로 그려야합니다. 

**한 층을 3m 로 가정하여 Material 의 SetTextureScale 값을 조정해주세요.**

**ex) 한 동의 높이가 10m 인 경우 = 10 / 3 = 3.3333 = 3층**

**Material.SetTextureScale("_BaseMap", new Vector2(1f, 3));**

아래의 그림과 비슷한 모양이 나와야합니다. 실 데이터와 차이가 있으므로 정확하게 일치하지는 않습니다.

![결과2](https://i.imgur.com/gbjA93E.png)

#### 평가기준

1. <b>아파트 단지에 텍스쳐 맵핑을 적용 (10점)

2. 주어진 텍스쳐 이미지를 문제에서 제시한 영역만큼 구분하여 적용 (20점)
   * 정점의 uv 좌표를 이용하여 구현해야 합니다. 텍스쳐 파일 나누어 구현하는 것은 인정되지 않습니다.

3. 적용된 텍스쳐를 문제에 제시한 방향 조건에 맞추어 적용 (30점)</b>

### 코드 설계시 우대되는 부분

유니티 라이프 사이클 함수 (Awake, Start, OnEnable, OnDisable, OnDestroy, Update) 이외의 직접 라이플 사이클을 구현 <b>(점수 획득한 문제에 가산점 +20점)</b>

MyFramework.cs 파일에 GameObject / Material 등의 리소스 생성 및 삭제가 구현되어야 합니다.

MyFramework.cs 파일에만 유니티 라이프 사이클 함수를 사용합니다.

리소스 생성 및 삭제의 순서가 명확해야합니다.

# 문제 해결 방법 (자유롭게 작성)

문제.1
- Json 파일을 Assets 폴더에서 읽어들이고 임포트한 Newtonsoft JSON 라이브러리로 Parsing하여 JSON 데이터를 사용합니다. (룸타입id, 동 같은 key가 있어 외부 라이브러리를 사용, 아스키코드라면 내장 라이브러리 JsonUtility  사용가능)
- GetCoordinate 함수를 통해 base64문자열을 byte 배열로 변환, btye 배열을 float 형 사이즈에 맞게 옮긴뒤 3개씩 point를 순서대로 x, z, y로 넣어 버텍스 배열을 Mesh에 전달합니다.
- 또 3개씩 버텍스를 묶어 Triangle 묶음을 만들어 MeshFilter.mesh에 전달합니다. 
- RecalculateBounds로 메쉬의 경계면 크기 바운딩 볼륨을 계산하고, RecalculateNormals를 통해 버텍스의 Normal값을 자동 계산합니다.
- 모든 버텍스를 묶어 모든 데이터가 하나의 메시로 묶이며 하나의 게임 오브젝트로 그려집니다.
- 참고로 스크립트는 오브젝트에 스크립트를 붙이지 않고 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] 을 통해 Scene 로드시 게임오브젝트를 만들고 필요한 컴포넌트를 추 가하였습니다. 유니티 라이프 사이클 함수 (Awake, Start, OnEnable, OnDisable, OnDestroy, Update) 이외의 직접 라이플 사이클을 구현 (점수 획득한 문제에 가산점 +20점) 라이플사이클을 사용하지는 않았는데 의도한 내용이랑 다를수 있습니다.

문제.2
- 텍스처링을 하기 위하여 Assets 폴더에서 텍스처를 읽어들이고 MeshRenderer.material에 전달합니다.
- 1번 조건에 맞게 Vector3.up, Vector3.down과 Normal을 Dot연산 후 값이 1과 같은 값은 (3)번을 쓰도록 정점에 uv를 텍스처 영역에 맞게 지정하였습니다. (uv <0.75,0,5> ~ <1,1>) 참고로 up, down 방향의 normal이 RecalculateNormals에서 얻은 값고 근사하지만 정확하지 않아 근사치 0.99 값까지 허용하였습니다. 같은 좌표의 버텍스가 미묘하게 달라서 발생한것 같아 오차수정하였습니다.
- 2번 조건에 맞게 Y축으로의 시계방향으로 Vector3.Forward와 normal의 사이각을 Degree로 구하고 180~220도 사이의 텍스처는 (1)번을 쓰도록 정점uv를 텍스처 영역에 맞게 지정하였습니다 (uv <0,높이> ~ <0.5,높이>)
- 높이 / 3으로 나오는 값을 V의 값으로 하여 텍스처가 층 개수에 따라 나오도록 설정하였습니다. (repeat 모드)
- 2번,3 조건이면서 UV를 할당한 좌표는 savedUVList에 저장하여 재사용하였고, 삼각형의 모든 버텍스가 저장되어 있지 않다면 초기시작 UV를 세팅하였고(사각형벽을 만들때 시작이 오른쪽부터라 우측방향의 U를 먼저세팅), 삼각형의 일부분이 저장되었다면 저장된 UV를 기준으로 Normal 방향과 좌,우 방향에 따라 설정하였습니다. 
- 3번 조건은 else로 (2)번을 쓰도록 정점uv를 텍스처 영역에 맞게 지정하였습니다 (uv <0.5,높이> ~ <0.75,높이>)
- 요구하는 결과 사진과 비슷하게 Orthograpic, 환경광으로 세팅하였고 Shadow Distance를 길게 설정하였습니다.
- 프로젝트를 열때 자동으로 ZedScene이 열리는게 아니므로 ZedScene를 먼저 열고 플레이 해야합니다.

참고.
결과 사진과 렌더링 사진이 다릅니다. 이유는 남서쪽을 바라보는 것이 모두 창문이 있는게 아니고 면의 타입에 따라 텍스처링이 달라져야하는데 조건이 부족합니다.
바닥과 동번호는 문제 조건에 없어 진행하지 않았습니다.
