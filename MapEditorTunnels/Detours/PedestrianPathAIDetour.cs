using MapEditorTunnels.Redirection;

namespace MapEditorTunnels.Detours
{
    [TargetType(typeof(PedestrianPathAI))]
    public class PedestrianPathAIDetour : PedestrianPathAI
    {
        [RedirectMethod]
        public override void GetElevationLimits(out int min, out int max)
        {
            //begin mod
            min = this.m_tunnelInfo == null ? 0 : -3;
            //end mod
            max = this.m_elevatedInfo != null || this.m_bridgeInfo != null ? 2 : 0;
        }


        [RedirectMethod]
        public override NetInfo GetInfo(float minElevation, float maxElevation, float length, bool incoming, bool outgoing, bool curved, bool enableDouble, ref ToolBase.ToolErrors errors)
        {
            if (this.m_invisible)
                return this.m_info;
            if ((double)maxElevation > (double)byte.MaxValue)
                errors = errors | ToolBase.ToolErrors.HeightTooHigh;
            //begin mod
            if (this.m_tunnelInfo != null && (double)maxElevation < -8.0)
                return this.m_tunnelInfo;
            if (this.m_slopeInfo != null && (double)minElevation < -8.0)
                return this.m_slopeInfo;
            //end mod
            if (this.m_bridgeInfo != null && (double)maxElevation > 25.0 && ((double)length > 45.0 && !curved) && (enableDouble || !this.m_bridgeInfo.m_netAI.RequireDoubleSegments()))
                return this.m_bridgeInfo;
            if (this.m_elevatedInfo != null && (double)maxElevation > 0.100000001490116)
                return this.m_elevatedInfo;
            if ((double)maxElevation > 4.0)
                errors = errors | ToolBase.ToolErrors.HeightTooHigh;
            return this.m_info;
        }

        [RedirectMethod]
        public override bool SupportUnderground()
        {
            if (this.m_tunnelInfo != null)
                //begin mod
                return true;
                //end mod
            return false;
        }
    }
}