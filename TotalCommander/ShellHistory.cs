using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TotalCommander
{
    /// <summary>
    /// The history of the shell explorer
    /// </summary>
    public class ShellHistory
    {
        #region Local variables

        private List<string> m_History;
        private int m_Current;

        #endregion

        internal ShellHistory()
        {
            m_Current = -1;
            m_History = new List<string>();
        }

        /// <summary>
        /// Clears the history.
        /// </summary>
        public void Clear()
        {
            if (m_History.Count > 0)
            {
                m_Current = -1;
                m_History.Clear();
            }

        }

        internal void Add(string folder)
        {
            int count = m_History.Count - m_Current - 1;
            if (count > 0)
            {
                m_History.RemoveRange(m_Current + 1, count);
            }
            //while (_Current < _History.Count - 1)
            //{
            //    _History.RemoveAt(_Current + 1);
            //}

            m_History.Add(folder);
            m_Current = m_History.Count - 1;
        }

        public string MoveBackward()
        {
            if (m_Current < 0)
            {
                throw new InvalidOperationException("Cannot navigate back");
            }
            m_Current -= 1;
            return m_History[m_Current];
        }

        public string MoveForward()
        {
            if (m_Current == m_History.Count - 1)
            {
                throw new InvalidOperationException("Cannot navigate forward");
            }
            m_Current += 1;
            return m_History[m_Current];
        }

        /// <summary>
        /// 뒤로 이동하여 이전 경로를 반환합니다. 이동할 수 없는 경우 빈 문자열을 반환합니다.
        /// </summary>
        public string Backward()
        {
            if (CanNavigateBack)
            {
                return MoveBackward();
            }
            return string.Empty;
        }

        /// <summary>
        /// 앞으로 이동하여 다음 경로를 반환합니다. 이동할 수 없는 경우 빈 문자열을 반환합니다.
        /// </summary>
        public string Forward()
        {
            if (CanNavigateForward)
            {
                return MoveForward();
            }
            return string.Empty;
        }

        internal bool CanNavigateBack
        {
            get { return m_Current > 0; }
        }

        internal bool CanNavigateForward
        {
            get { return m_Current != m_History.Count - 1; }
        }
    }
}
