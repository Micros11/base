using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRManagement.HRRecruitmentAssignment;

namespace DirRX.HRManagement
{
  partial class HRRecruitmentAssignmentClientHandlers
  {

    public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
    {
      var isMeetingBlock = _obj.BlockUid == Constants.RecruitmentTask.HRMeetingBlockId;
      _obj.State.Properties.Managers.IsVisible = isMeetingBlock;
      _obj.State.Properties.Managers.IsRequired = isMeetingBlock;
    }

  }
}