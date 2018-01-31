﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Data;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace cc.newspring.AttendedCheckIn.Workflow.Action.CheckIn
{
    /// <summary>
    /// Selects the available grouptype, group, location and schedule if it matches their previous attendance
    /// </summary>
    [Description( "Selects the grouptype, group, location and schedule for each person based on what they last checked into." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Select By Last Attended" )]
    public class SelectByLastAttended : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, Rock.Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                return false;
            }

            var family = checkInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            if ( family != null )
            {
                foreach ( var person in family.People.Where( f => f.Selected && !f.FirstTime ).ToList() )
                {
                    if ( person.LastCheckIn != null )
                    {
                        var groupType = person.GroupTypes.FirstOrDefault( gt => gt.Selected || gt.LastCheckIn == person.LastCheckIn );
                        if ( groupType != null )
                        {
                            groupType.PreSelected = true;
                            groupType.Selected = true;

                            var group = groupType.Groups.FirstOrDefault( g => g.Selected || g.LastCheckIn == person.LastCheckIn );
                            if ( group != null )
                            {
                                group.PreSelected = true;
                                group.Selected = true;

                                var location = group.Locations.FirstOrDefault( l => l.Selected || l.LastCheckIn == person.LastCheckIn );
                                if ( location != null )
                                {
                                    location.PreSelected = true;
                                    location.Selected = true;

                                    var schedule = location.Schedules.FirstOrDefault( s => s.Selected || s.LastCheckIn == person.LastCheckIn );
                                    if ( schedule != null )
                                    {
                                        schedule.PreSelected = true;
                                        schedule.Selected = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
    }
}
