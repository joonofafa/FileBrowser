# 코딩 가이드라인

이 문서는 FileBrowser 프로젝트의 코딩 스타일과 다국어 처리에 관한 가이드라인을 제공합니다.

## 코드 스타일 및 다국어 처리 가이드라인

### 1. 소스 코드 주석 처리
- **모든 코드 주석은 영어로 작성합니다.**
- 한국어로 된 주석을 발견하면 영어로 번역해야 합니다.
- 주석은 간결하고 명확하게 작성합니다.
- 클래스, 메서드, 프로퍼티에는 XML 문서 주석(`///`)을 사용합니다.

### 2. 문자열 처리
- **하드코딩된 한국어 문자열은 허용되지 않습니다.**
- 모든 UI 표시 문자열은 `StringResources.cs`를 통해 관리해야 합니다.
- 새로운 문자열을 추가할 때는 영어(en)와 한국어(ko) 버전을 모두 추가해야 합니다.
- 디버그용 로그 메시지는 영어로 작성합니다.

### 3. StringResources 사용 방법
- 코드에서 문자열 표시: `StringResources.GetString("키")`
- 매개변수가 있는 문자열: `StringResources.GetString("키", 매개변수1, 매개변수2)`
- 새 문자열 추가 시 `StringResources.cs` 파일의 `enResources`와 `koResources` 모두에 추가

### 4. 코드 네이밍 규칙
- 클래스, 메서드, 프로퍼티: PascalCase (예: `ClassName`, `MethodName`)
- 지역 변수, 매개변수: camelCase (예: `localVariable`, `parameterName`)
- 멤버 변수: 접두사 `m_`를 사용 (예: `m_memberVariable`)
- 상수, 정적 readonly 필드: 대문자와 밑줄 (예: `MAX_VALUE`, `DEFAULT_SIZE`)

### 5. 파일 구조
- 클래스 당 하나의 파일을 사용합니다.
- 파일 이름은 클래스 이름과 일치해야 합니다.
- 네임스페이스는 프로젝트 구조를 반영해야 합니다.

## 예제

### 올바른 문자열 사용 예제
```csharp
// 잘못된 방법 - 하드코딩된 문자열
MessageBox.Show("파일을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

// 올바른 방법 - StringResources 사용
MessageBox.Show(
    StringResources.GetString("FileNotFound"), 
    StringResources.GetString("Error"), 
    MessageBoxButtons.OK, 
    MessageBoxIcon.Error
);
```

### 새로운 문자열 추가 예제
```csharp
// StringResources.cs 파일에 추가
// English resources
enResources["NewFeatureTitle"] = "New Feature";
enResources["NewFeatureDescription"] = "This is a description of the new feature.";

// Korean resources
koResources["NewFeatureTitle"] = "새 기능";
koResources["NewFeatureDescription"] = "새 기능에 대한 설명입니다.";
```

## 코드 리뷰 체크리스트
- [ ] 모든 주석이 영어로 작성되었는가?
- [ ] UI에 표시되는 모든 문자열이 StringResources를 통해 관리되는가?
- [ ] 새로 추가된 문자열에 영어와 한국어 버전이 모두 있는가?
- [ ] 코드 네이밍 규칙을 준수하였는가?
- [ ] XML 문서 주석이 적절히 사용되었는가? 