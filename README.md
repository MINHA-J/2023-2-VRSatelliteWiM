# Satellite WIM Object Selection in VR 

<aside>💡 경희대학교 2023-2학기 졸업논문</aside><br/>
![Untitled](https://github.com/MINHA-J/2023-2-VRSatelliteWiM/assets/64896960/d9c9bf83-2f36-4b91-9a79-ccdc0e3f2302)
<br/><br/>

### C O N T E N T S
1. [개요](#1️⃣-개요)
2. [서베이 및 개발 과정](#2️⃣-서베이-및-개발-과정)
3. [사용자 실험 설계](#3️⃣-사용자-실험-설계)
4. [결과 및 분석](#4️⃣-결과-및-분석)

<br/><br/>
## 1️⃣ 개요

### ▶문제상황

- 가상 객체와 빠르고 정확하게 상호작용하는 것은 매우 중요한 요소이다.
- 그러나 종종 VR 내에서 일어나는 상호작용은 한계를 가진다.
    - 사용자의 행동 반경을 벗어나, 먼 거리에 놓여 있는 가상 객체와 상호작용하기 위해서는 **사용자의 이동이 필요**하다는 문제를 가진다.
    - 3D 환경에서 다른 가상 물체에 의해 가려져 사용자의 시야에 보이지 않을 경우, 해당 물체와 상호작용하기 위해서는 적지 않은 노력이 필요하다.
<br/><br/>
### ▶Satellite WiM

- 사용자는 가상 환경의 요약본이 표현된 행성 (Planet)을 통해 원하는 구역(ROI)을 선택한다.
- 생성된 위성 (Satellite)으로 ROI를 제어할 수 있는 Satellite WIM를 제안한다.
- 미니어처 및 포털의 세계를 기반으로 ROI 공간은 사용자의 가까이에 표현되며, 이를 통해 가상 객체와 상호작용할 수 있다.
- 이를 통해 사용자는 다양한 규모, 여러 범위의 공간과 직접적으로 상호 작용하는 것처럼 느낄 수 있다.
<br/><br/>
### ▶Tool

- Oculus Quest2 및 Leap Motion
- Unity(2021.3.61f1)
<br/><br/><br/><br/>
## 2️⃣ 서베이 및 개발 과정

### ▶Servey

Google Scholar, IEEE Xplore, ACM Digital Library 등을 통해 학술 논문 및 서적을 검색하고 참고

[Paper Search를 정리한 자료](https://docs.google.com/spreadsheets/d/1NWEct7s8TL5R0UrFuEt9Ze1KGfUtb7FcnoqdsQK4r3w/edit?usp=sharing)
<br/><br/>
### ▶개발

- 행성(Planet): 사용자가 존재하는 Plane한 가상 환경을 구의 표면에 mapping한 제2 시점을 일컫는 말
- 위성(Satellite): 선택한 ROI를 행성 주변에서 확인하고, 제어할 수 있게끔 함
<br/><br/>
### 행성(Planet)

![1](https://github.com/MINHA-J/2023-2-VRSatelliteWiM/assets/64896960/e14af796-ad4c-48d8-92bb-c318ba487ec1)

- 가상 환경을 어느정도 커버할 수 있는 Volume의 구 형태가 Plane으로 mapping되어 구현된다.
- Unity의 ScriptableRenderContext, RenderingData를 이용하여 Update되며, 해당 구 내부에 있는 가상 환경의 물체들을 Clipping render하여 행성 내부에 평면적으로 mapping한다.
- 행성과 사용자의 눈(Camera) 사이의 거리를 좁혀 사용자가 원하는 공간(ROI)을 세부적으로 확인할 수 있으며, 거리를 늘여 전체적인 공간을 확인할 수 있다.
    
    ![2](https://github.com/MINHA-J/2023-2-VRSatelliteWiM/assets/64896960/22a23271-3906-4f76-a9b8-e60a5ecc60e7)
    
- Player가 바라보는 구역은 파란 구의 indicator로 확인 가능하다. 행성의 위치 또는 사용자의 시점을 이동하여 Portal를 생성하고자 하는 ROI를 설정할 수 있다.
- 원하는 ROI의 지점을 설정한 뒤, 왼손으로 2초간 엄지와 검지로 집는 자세(Pinch)를 유지하여 해당 위치에 포탈(Portal)을 생성한다.
<br/><br/>
### 위성(Satellite)

![3](https://github.com/MINHA-J/2023-2-VRSatelliteWiM/assets/64896960/8f760b93-abc3-4626-a5ff-4f2e0cca6cdb)

- 행성 위에는 위성(Satellite)과 상호작용 가능한 포탈(Portal)이 생성되고, 실제 가상환경의 관심 공간에는 사용자와의 상호작용을 위한 포탈이 생성된다.
- 행성 위에 생성되는 Satellite는 관심 구역의 위치를 행성에서 빠르게 확인할 수 있다.
- Satellite를 손으로 잡아 상하로 움직여 상호작용을 위한 포탈의 크기를 조절하여 원하는 객체 선택에 유리하게 조절할 수 있으며, 좌우로 움직여 관심 영역의 위치를 재설정할 수 있다.
<br/><br/>
### Portal

![4](https://github.com/MINHA-J/2023-2-VRSatelliteWiM/assets/64896960/7f815636-4880-47ff-bf6a-cd7fd96a3250)

- 포탈은 Shader, 색상, 효과를 통해서 사용자에게 직관적으로 인식될 수 있도록 구현됨
- 포탈 내부의 시각적인 객체의 복제는Unity의 scriptable render pipeline에서 추가된 추가적인 Render pass를 통한 것이므로 객체가 중복되는 문제가 발생하지 않는다는 장점을 갖고 있다.
<br/><br/><br/><br/>
## 3️⃣ 사용자 실험 설계

Satellite WiM(Portal&WiM)의 성능을 평가하기 위해서 Portal Only와 Teleport 방식과 비교하는 실험을 설계함.
<br/><br/>
### [실험1] Near Distance Object

![5](https://github.com/MINHA-J/2023-2-VRSatelliteWiM/assets/64896960/5a1f2706-000e-4a08-8595-9276cd0b7119)

- 실내 공간 크기의 가상현실에서 Satellite WiM의 성능을 평가한다.
- ROI는 A(파란색), B(빨간색)의 2개 구역이 존재하며 포탈을 생성하여 가상 객체를 A→B로 옮긴다.
- 가까운 거리, 그러나 가려진 가상객체 선택 상황에서 개선 정도를 파악한다.

**독립변인**

- **기술**: Poros[14](Portal Only) 또는 Satellite WiM(Portal&WiM)

**통제변인**

- **크기**: 실험은 실내 사이즈의 공간에서 수행된다. (15⨉15)
- **장애물**: 사용자의 시야를 가리는 높이의 가상객체의 사이에서 선택해야 하는 가상객체가 생성된다.
- **포탈**: 포탈은 ROI 선택 후 사용자의 근접한 한 구역에 생성되며, 여러 번 재설정할 수 있다. 최대 1개까지 생성된다.
- **반복**: 각 기술마다1- 3회까지 반복한다.

**종속변인**

- **시간**: 실험자는 모든 3번의 가상 객체 옮기기가 끝난 후 버튼을 누른다. 그때까지 소요된 시간
- **포탈 생성 시간**: 포털 생성을 위해 ROI 구역을 확인하고, 가상 객체 선택 시스템을 통해 포탈을 생성하는 데까지 소요된 시간
- **포탈 재설정 횟수**: 실험자가 원하는 ROI에 포털을 세팅하기 위해 설정한 횟수
- **포탈 생성 정확도**: 가상 객체 선택 시스템을 통해 포탈을 생성하였을 때, 해당 포탈과ROI(A, B구역) 중심부와의 거리 차이
- **인지 부하**: NASA TLX(very low~very high)
- **선호**: willing to use(0~7)

**가설**

- Satellite WiM(Portal&WiM)을 활용한다면, 빠른 시간 내에 ROI에 Portal를 생성할 수 있음.
- Satellite WiM(Portal&WiM)을 활용한다면, ROI에 정확하게 Portal을 생성할 수 있음.
- Satellite WiM(Portal&WiM)을 활용한다면, 사용자의 과한 움직임(Hand)을 요구하지 않음.
- Satellite WiM(Portal&WiM)을 활용하는 것이 사용자는 더 직관적이고 편리하다고 느낌.
<br/><br/>
### [실험2] Far Distance Object

![6](https://github.com/MINHA-J/2023-2-VRSatelliteWiM/assets/64896960/1436627e-677e-43f7-8ec2-ccf201e72032)

- 사용자의 시야에 겨우 보일 만큼 큰 공간 크기의 가상현실에서 Satellite WiM의 성능을 평가한다.
- ROI는 3개의 A(파란색), 하나의 B(빨간색)의 4개 구역이 존재하며 포탈을 여러 개 생성하여 가상 객체를 A→B로 옮긴다.
- 먼 거리의 가상객체 선택 상황에서 개선 정도를 파악한다.

**독립변인**

- **기술**: Teleport또는 Satellite WiM(Portal&WiM)

**통제변인**

- **크기**: 실험은 사용자에 시야에 겨우 닿을 만한 넓은 사이즈의 공간에서 수행된다. (100⨉100)
- **포탈**: 포탈은 ROI 선택 후 사용자의 근접한 한 구역에 생성되며, 여러 번 재설정할 수 있다. 최대 3개까지 생성된다
- **반복**: 각 기술마다1- 3회까지 반복한다.

**종속변인**

- **시간**: 실험자는 모든 3번의 가상 객체 옮기기가 끝난 후 버튼을 누른다. 그때까지 소요된 시간
- **인지 부하**: NASA TLX(very low~very high)
- **피로도**: 7-point Likert scale
- **선호**: willing to use(0~7)
- **사용성**: 포탈 생성 과정에서의 사용성, 포탈과의 인터렉션 과정에서의 사용성에 관한 실험자의 주관적인 의견

**가설**

- Satellite WiM(Portal&WiM)을 활용한다면, 빠른 시간 내에 분산된 오브젝트와 상호작용할 수 있음.
- Satellite WiM(Portal&WiM)을 활용한다면, 사용자의 VR환경에서의 인지부하를 감소시킬 수 있음.
- Satellite WiM(Portal&WiM)을 활용하는 것이 사용자는 더 직관적이고 편리하다고 느낌.
<br/><br/><br/><br/>
## 4️⃣ 결과 및 분석

### 실험 결과

위 설계 중, [실험 1]을 Unity project로 구현하여 사용자 테스트를 진행함

테스트에 참여한 실험자들의 VR 경험 정도는 다양했다. 

사용자는 Satellite WIM 시스템(실험군)과Satellite WIM를 적용하지 않은 기존의 Opensource[14] 또는 Teleport (대조군)을 모두 체험한다.

- **시간**
    - 기술에 따른 시간의 유의미한 영향은 없었다.
    - Poros(Only portal)의 경우 평균: 52.36, 표준편차: 20.43
    - Satellite WiM(Portal&WiM)의 경우 평균: 44.74, 표준편차: 12.31
    - **Satellite WiM이 객체 선택 옮기기를 약간 더 빠르게 수행**할 수 있음
- **포탈 생성 시간**
    - 기술에 따른 포털 생성 시간의 유의미한 차이가 존재했다.
    - Poros(Only portal)의 경우 평균: 21.30, 표준편차: 10.08
    - Satellite WiM(Portal&WiM)의 경우 평균: 13.52, 표준편차: 6.57
    - **Satellite WiM을 활용하는 경우가 더 빠르게 포탈을 ROI에 설정**할 수 있음
- **포탈 재설정 횟수**
    - 기술에 따른 포털 생성 시간의 유의미한 차이는 없었다.
    - Poros(Only portal)의 경우 평균: 1.75, 표준편차: 0.5
    - Satellite WiM(Portal&WiM)의 경우 평균: 1.25, 표준편차: 0.5
    - **Satellite WiM을 활용하는 경우가 더 적은 횟수로 포탈을 세팅**하였으나, 큰 차이는 없음
- **포탈 생성 정확도**
    - 기술에 따른 포털 생성 시간의 유의미한 차이가 존재하였다.
    - Poros(Only portal)의 경우 평균: 8.06, 표준편차: 1.7
    - Satellite WiM(Portal&WiM)의 경우 평균: 1.45, 표준편차: 2.21
    - **Satellite WiM을 활용하는 경우가 더 정확한 ROI로 포탈을 생성**할 수 있는 것을 확인
<br/><br/>
### 의의

- 사용자는 신체적, 물리적으로 닿지 않는 먼 거리의 객체를 직접 선택하고 상호작용할 수 있게 된다.
- 다른 가상 객체에 의해 가려진 경우에도 Satellite를 통한 시점 이동이나 포탈의 회전을 통해 해당 객체에 접근할 수 있다.
- Satellite WIM 시스템은 두 손의 자유로움이 보장된다
- 가상 환경에서의 객체 선택을 개선하여 더 큰 몰입감을 형성할 수 있을 것이다.
- 실험 1, Near distance object를 직접 구현하여 소수의 인원을 대상으로 간단한 사용자 테스트를 진행한 결과, Satellite WIM을 통해 가상 객체 선택 시간 및 정확도 문제를 상당히 향상시킬 수 있음을 알 수 있었다.
<br/><br/>
### 한계점

- 실험 설계 과정에서 조건의 Counter Balancing 및 주관적 요소의 종속 변인 등을 보완하여 더욱 많은 유저 테스트를 진행하고자 함
- 향후에는 Satellite WIM 시스템을 조작하기 쉽도록 하는 다양한 방안에 대하여 연구하고자 함
- 해당 시스템을 적극적으로 활용할 수 있는 애플리케이션 환경을 구현하여 활용성을 입증하고자 한다.
<br/><br/>
## 🔗 관련 자료 및 링크

`Github`

https://github.com/MINHA-J/2023-2-VRSatelliteWiM

`Youtube`

https://drive.google.com/file/d/1YTz7XsgCMRwVxunY5xBdybqcvK1sBUCJ/view?usp=sharing

`참고자료`

https://github.com/henningpohl/poros
