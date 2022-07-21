using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using Sungero.Company;
using DirRX.HRManagement.HRDocument;

namespace DirRX.HRManagement.Client
{
  partial class HRDocumentActions
  {
    public override void SendForApproval(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Sungero.Docflow.PublicFunctions.Module.ApproveWithAddenda(_obj, new List<Sungero.Docflow.IOfficialDocument>(), null, Sungero.Company.Employees.As(Users.Current), false, false, string.Empty);
      base.SendForApproval(e);
    }

    public override bool CanSendForApproval(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return base.CanSendForApproval(e);
    }

  }

}