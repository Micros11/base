using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.HRLite.RefusalAcqAssignment;

namespace DirRX.HRLite.Client
{
  partial class RefusalAcqAssignmentAnyChildEntityCollectionActions
  {
    public override void DeleteChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.DeleteChildEntity(e);
    }

    public override bool CanDeleteChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      var root = RefusalAcqAssignments.As(e.RootEntity);
      return (root != null && _all == root.RefusalResult) 
        ? false 
        : base.CanDeleteChildEntity(e);
    }

  }

  partial class RefusalAcqAssignmentAnyChildEntityActions
  {
    public override void CopyChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.CopyChildEntity(e);
    }

    public override bool CanCopyChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      var root = RefusalAcqAssignments.As(e.RootEntity);
      return (root != null && _all == root.RefusalResult) 
        ? false 
        : base.CanCopyChildEntity(e);
    }

    public override void AddChildEntity(Sungero.Domain.Client.ExecuteChildCollectionActionArgs e)
    {
      base.AddChildEntity(e);
    }

    public override bool CanAddChildEntity(Sungero.Domain.Client.CanExecuteChildCollectionActionArgs e)
    {
      var root = RefusalAcqAssignments.As(e.RootEntity);
      return (root != null && _all == root.RefusalResult) 
        ? false 
        : base.CanAddChildEntity(e);
    }

  }

  partial class RefusalAcqAssignmentActions
  {
    public virtual void Complete(Sungero.Workflow.Client.ExecuteResultActionArgs e)
    {
      
    }

    public virtual bool CanComplete(Sungero.Workflow.Client.CanExecuteResultActionArgs e)
    {
      return true;
    }

  }

}