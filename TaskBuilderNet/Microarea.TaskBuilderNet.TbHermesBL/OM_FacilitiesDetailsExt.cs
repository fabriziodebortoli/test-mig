using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.TbHermesBL;

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    public partial class OM_FacilitiesDetails
    {
        //-------------------------------------------------------------------------------
        public static List<OM_FacilitiesDetails> GetCommitmentFacilities(TbSenderDatabaseInfo cmp, int commitmentId)
        {
            List<OM_FacilitiesDetails> list = null;
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                list = GetCommitmentFacilities(db, commitmentId);
            }

            return list;
        }
        public static List<OM_FacilitiesDetails> GetCommitmentFacilities(MZP_CompanyEntities db, int commitmentId)
        {
            List<OM_FacilitiesDetails> list = new List<OM_FacilitiesDetails>();

            var items = from f in db.OM_FacilitiesDetails
                        where f.CommitmentId == commitmentId
                        select f;
            list.AddRange(items);
            
            return list;
        }

        //-------------------------------------------------------------------------------
        public static List<OM_FacilitiesDetails> GetFacilitiesFromDateRange(TbSenderDatabaseInfo cmp, int facilityId, DateTime startTime, DateTime endTime)
        {
            List<OM_FacilitiesDetails> list = null;
            using (var db = ConnectionHelper.GetHermesDBEntities(cmp))
            {
                list = GetFacilitiesFromDateRange(db, facilityId, startTime, endTime);
            }
            return list;
        }
        public static List<OM_FacilitiesDetails> GetFacilitiesFromDateRange(MZP_CompanyEntities db, int facilityId, DateTime startTime, DateTime endTime)
        {
            List<OM_FacilitiesDetails> list = new List<OM_FacilitiesDetails>();
            var items = from f in db.OM_FacilitiesDetails
                        where f.FacilityId == facilityId
                        && (((startTime.Date <= f.StartDate) && (f.StartDate <= endTime.Date)) ||
                            ((startTime.Date <= f.EndDate ) && (f.EndDate <= endTime.Date)))
                        select f;
            list.AddRange(items);

            return list;
        }

        //-------------------------------------------------------------------------------
        public static OM_FacilitiesDetails NewItem(MZP_CompanyEntities db, OM_FacilitiesDetails fdet, DateTime start, DateTime end, int commitmentId,
                                                Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_FacilitiesDetails facdet = null;

            try
            {
                facdet = CreateItem(fdet);
                facdet.CommitmentId = commitmentId;
                facdet.StartDate = start.Date;
                facdet.EndDate = end.Date;
                facdet.StartTime = new DateTime(1899, 12, 30) + start.TimeOfDay;
                facdet.EndTime = new DateTime(1899, 12, 30) + end.TimeOfDay;

                db.OM_FacilitiesDetails.Add(facdet);
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }

            return facdet;
        }

        //-------------------------------------------------------------------------------
        public static OM_FacilitiesDetails UpdateItem(MZP_CompanyEntities db, OM_FacilitiesDetails fdet, DateTime start, DateTime end, 
                                                Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            OM_FacilitiesDetails facdet = null;

            try
            {
                facdet = CreateItem(fdet);
                facdet.StartDate = start.Date;
                facdet.EndDate = end.Date;
                facdet.StartTime = new DateTime(1899, 12, 30) + start.TimeOfDay;
                facdet.EndTime = new DateTime(1899, 12, 30) + end.TimeOfDay;

                db.OM_FacilitiesDetails.Remove(fdet);
                db.OM_FacilitiesDetails.Add(facdet);
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }

            return facdet;
        }

        //-------------------------------------------------------------------------------
        public static OM_FacilitiesDetails CreateItem(OM_FacilitiesDetails facilityDetails)
        {
            OM_FacilitiesDetails fd = new OM_FacilitiesDetails();

            fd.CommitmentId = facilityDetails.CommitmentId;
            fd.FacilityId = facilityDetails.FacilityId;
            fd.StartDate = facilityDetails.StartDate;
            fd.EndDate = facilityDetails.EndDate;
            fd.StartTime = facilityDetails.StartTime;
            fd.EndTime = facilityDetails.EndTime;
            fd.Notes = facilityDetails.Notes;
            fd.WorkerId = facilityDetails.WorkerId;
            
            DateTime now = DateTime.Now;
            fd.TBCreated = now;
            fd.TBModified = now;
            fd.TBCreatedID = facilityDetails.TBCreatedID;
            fd.TBModifiedID = facilityDetails.TBCreatedID;

            fd.TBGuid = Guid.NewGuid();

            return fd;
        }

        //-------------------------------------------------------------------------------
        public static void RemoveFacilitiesDetails(MZP_CompanyEntities db, int commitmentID,
            Microarea.TaskBuilderNet.TbHermesBL.HermesEngine.TreatExceptionDelegate excLogger)
        {
            try
            {
                var query = from fd in db.OM_FacilitiesDetails
                            where (fd.CommitmentId == commitmentID)
                            select fd;

                foreach (OM_FacilitiesDetails currFD in query)
                {
                    //Delete All Events??
                    db.OM_FacilitiesDetails.Remove(currFD);
                }
            }
            catch (Exception ex)
            {
                if (excLogger != null)
                    excLogger(ex);
            }
        }


    }
}
