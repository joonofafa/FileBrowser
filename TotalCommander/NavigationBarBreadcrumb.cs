using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TotalCommander
{
    /// <summary>
    /// 파일 탐색기 상단에 표시되는 경로 내비게이션 바 컨트롤
    /// </summary>
    public class NavigationBarBreadcrumb : Control
    {
        // 경로 구분자
        private static readonly char[] PathSeparators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        
        // 현재 경로
        private string _currentPath;
        
        // 경로 버튼 목록
        private List<Button> _pathButtons = new List<Button>();
        
        // 너비 조정을 위한 패널
        private Panel _buttonPanel;
        
        /// <summary>
        /// 경로가 변경될 때 발생하는 이벤트
        /// </summary>
        public event EventHandler<string> PathChanged;
        
        /// <summary>
        /// 현재 경로
        /// </summary>
        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                if (_currentPath != value)
                {
                    _currentPath = value;
                    UpdatePathButtons();
                }
            }
        }
        
        /// <summary>
        /// 생성자
        /// </summary>
        public NavigationBarBreadcrumb()
        {
            // 기본 설정
            DoubleBuffered = true;
            Height = 24;
            
            // 버튼 컨테이너 패널 생성
            _buttonPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            
            Controls.Add(_buttonPanel);
        }
        
        /// <summary>
        /// 경로에 따라 버튼 업데이트
        /// </summary>
        private void UpdatePathButtons()
        {
            // 이전 버튼 제거
            foreach (var button in _pathButtons)
            {
                _buttonPanel.Controls.Remove(button);
                button.Dispose();
            }
            
            _pathButtons.Clear();
            
            if (string.IsNullOrEmpty(_currentPath))
                return;
                
            // 경로 파싱
            string[] pathParts;
            
            if (_currentPath.Contains(":"))
            {
                // 루트 드라이브부터 시작
                pathParts = _currentPath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
                if (pathParts.Length > 0 && pathParts[0].EndsWith(":"))
                {
                    pathParts[0] = pathParts[0] + "\\";
                }
            }
            else
            {
                // 네트워크 경로 또는 상대 경로
                pathParts = _currentPath.Split(PathSeparators, StringSplitOptions.RemoveEmptyEntries);
            }
            
            // 각 경로 부분에 대한 버튼 생성
            int left = 0;
            string currentFullPath = "";
            
            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];
                
                // 전체 경로 구성
                if (i == 0 && part.EndsWith(":\\"))
                {
                    currentFullPath = part;
                }
                else
                {
                    if (string.IsNullOrEmpty(currentFullPath))
                    {
                        currentFullPath = part;
                    }
                    else
                    {
                        currentFullPath = Path.Combine(currentFullPath, part);
                    }
                }
                
                // 경로 버튼 생성
                var button = new Button
                {
                    Text = part,
                    Tag = currentFullPath,
                    Left = left,
                    Height = Height - 4,
                    Top = 2,
                    FlatStyle = FlatStyle.Flat
                };
                
                // 버튼 크기 자동 조정
                button.AutoSize = true;
                button.Click += PathButton_Click;
                
                _pathButtons.Add(button);
                _buttonPanel.Controls.Add(button);
                
                left += button.Width + 5;
                
                // 구분자 추가 (마지막 경로 제외)
                if (i < pathParts.Length - 1)
                {
                    var separator = new Label
                    {
                        Text = ">",
                        Left = left,
                        Height = Height - 4,
                        Top = 2,
                        AutoSize = true
                    };
                    
                    _buttonPanel.Controls.Add(separator);
                    left += separator.Width + 5;
                }
            }
        }
        
        /// <summary>
        /// 경로 버튼 클릭 이벤트 핸들러
        /// </summary>
        private void PathButton_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;
            string path = (string)button.Tag;
            
            // 경로 변경 이벤트 발생
            PathChanged?.Invoke(this, path);
        }
        
        /// <summary>
        /// 컨트롤 크기 변경 시 버튼 업데이트
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdatePathButtons();
        }
    }
} 