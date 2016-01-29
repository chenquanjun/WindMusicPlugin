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

        public bool IsKeywordExist(string keyword)
        {
            //keyword 检查发生在搜索之前
            bool isExist = false;
            for (int i = 0; i < m_demandMusicQue.Count; i++)
            {
                var tmpInfo = (DemandInfo)m_demandMusicQue[i];
                if (tmpInfo.Keyword == keyword)
                {
                    isExist = true;
                    break;
                }
            }
            return isExist;
        }

        public bool IsSongIdExist(UInt32 songId, UInt32 stopQueueId)
        {
            Debug.Assert(stopQueueId != 0, "queueid should not be zero");
            //songId 检查发生在搜索之后，获取资料之前，此时已加入队列，因此有stop queue id
            bool isExist = false;

            for (int i = 0; i < m_demandMusicQue.Count; i++)
            {
                var tmpInfo = (DemandInfo)m_demandMusicQue[i];
                if (tmpInfo.QueueId == stopQueueId) //找到自己了，跳出循环
                {
                    break;
                }
                var songInfo = tmpInfo.SongInfo;
                if (songInfo != null && songInfo.Id == songId)
                {
                    isExist = true;
                    break;
                }
            }
            return isExist;
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
