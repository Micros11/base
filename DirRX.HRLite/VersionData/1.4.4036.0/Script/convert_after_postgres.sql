DO $$
begin
  if exists (SELECT * FROM information_schema.tables WHERE TABLE_NAME = 'sungerowftaskpdfconverterror')
  and exists (SELECT * FROM information_schema.columns WHERE TABLE_NAME = 'sungero_wf_task' AND COLUMN_NAME = 'converterror1_hrlite_dirrx' )
  and exists (SELECT * FROM information_schema.columns WHERE TABLE_NAME = 'sungero_wf_task' AND COLUMN_NAME = 'converterror_hrlite_dirrx')
  and exists (SELECT * FROM information_schema.columns WHERE TABLE_NAME = 'sungero_wf_task' AND COLUMN_NAME = 'converterror_hrmanag_dirrx')

  then
    UPDATE Sungero_WF_Task 
    SET 
      ConvertError1_HRLite_DirRX = tmp.ConvertError1_HRLite_DirRX,
      ConvertError_HRLite_DirRX = tmp.ConvertError_HRLite_DirRX,
      ConvertError_HRManag_DirRX = tmp.ConvertError_HRManag_DirRX
      FROM SungeroWfTaskPdfConvertError tmp
    WHERE tmp.Id = Sungero_WF_Task.id;
 
    DROP TABLE SungeroWfTaskPdfConvertError;
  end if; 
 
end$$;