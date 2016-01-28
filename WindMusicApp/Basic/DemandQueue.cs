using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace WindMusicApp
{
    class DemandQueue
    {
        private ArrayList m_demandMusicQue;
        private int m_maxNum;

        public DemandQueue(int maxNum)
        {
            m_demandMusicQue = new ArrayList();
            m_maxNum = maxNum;
        }

        public bool isFull()
        {
            return m_demandMusicQue.Count >= m_maxNum;
        }

        public void Add(DemandInfo info){
            Debug.Assert(!isFull(), "should check full first");
            m_demandMusicQue.Add(info);
        }

        public void Remove(DemandInfo info)
        {
            m_demandMusicQue.Remove(info);
        }

        public void RemoveById(UInt32 queueId)
        {
            GetInfo(queueId, true);
        }

        public DemandInfo GetInfo(UInt32 queueId, bool isRemove = false)
        {
            DemandInfo info = null;
            int index = -1;
            for (int i = 0; i < m_demandMusicQue.Count; i++)
            {
                var tmpInfo = (DemandInfo)m_demandMusicQue[i];
                if (tmpInfo.QueueId == queueId)
                {
                    info = tmpInfo;
                    index = i;
                    break;
                }
            }

            if (isRemove && index > -1)
            {
                m_demandMusicQue.RemoveAt(index);
            }
            return info;
        }

        public DemandInfo First()
        {
            if (m_demandMusicQue.Count == 0)
            {
                return null;
            }

            return (DemandInfo)m_demandMusicQue[0];
        }
    }
}
