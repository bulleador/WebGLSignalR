using System.Collections.Generic;
using PlayFab.MultiplayerModels;

namespace Lobby
{
    public class MemberEntityEqualityComparer : IEqualityComparer<Member>
    {
        public bool Equals(Member x, Member y)
        {
            if (x == null && y == null)
                return true;

            if (x == null || y == null)
                return false;
            
            return x.MemberEntity.Id == y.MemberEntity.Id;
        }

        public int GetHashCode(Member obj)
        {
            return obj.MemberEntity.Id.GetHashCode();
        }
    }
}