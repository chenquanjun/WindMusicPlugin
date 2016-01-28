using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindMusicApp
{
    public delegate void MusicEventAddFolderHandler(string folderName);
    public delegate void MusicEventRemoveFolderHandler(string folderName);

    public delegate void MusicEventDemandInfoHandler(DemandInfo info);

    public delegate void MusicEventMusicPlayStateHandler(UInt32 playId, MusicPlayState playState);

    public delegate void MusicEventMusicPlayDurationHandler(UInt32 playId, double curDur, double totalDur);

}
