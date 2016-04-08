namespace HydroTech.Data
{
    public class HydroPartList : HydroPartListBase<Part>
    {
        #region Overrides
        protected override Vessel GetVessel(Part item)
        {
            return item.vessel;
        }
        #endregion
    }
}