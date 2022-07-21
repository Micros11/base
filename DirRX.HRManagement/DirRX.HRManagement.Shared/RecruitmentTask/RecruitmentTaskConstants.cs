using System;
using Sungero.Core;

namespace DirRX.HRManagement.Constants
{
  public static class RecruitmentTask
  {
    
    /// <summary>
    /// ИД блока организации встречи.
    /// </summary>
    public const string HRMeetingBlockId = "4";

    /// <summary>
    /// ИД блока организации приема.
    /// </summary>
    public const string HiringBlockId = "10";
    
    /// <summary>
    /// ИД блока утверждения директором по персоналу.
    /// </summary>
    public const string ApproveBySeoBlockId = "6";
    
    /// <summary>
    /// ИД блока рассмотрения кандидата руководителями.
    /// </summary>
    public const string ManagerBlockId = "3";
    
    /// <summary>
    /// ИД блока заполнения отчета о кандидате руководителями.
    /// </summary>
    public const string ManagerReportBlockId = "5";
    
    /// <summary>
    /// ИД блока проверки СБ.
    /// </summary>
    public const string SBBlockId = "9";
    
    /// <summary>
    /// ИД блока фиксации решения по трудоустройству кандидата специалистом отдела кадров.
    /// </summary>
    public const string HRMeetingResultBlockId = "14";
    
    /// <summary>
    /// ИД блока организации проверки СБ\сообщения об отказе.
    /// </summary>
    public const string HRSBResultBlockId = "7";
  }
}