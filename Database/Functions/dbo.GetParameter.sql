CREATE FUNCTION dbo.GetParameter
(
    @ParameterName                      varchar(50)
)
    RETURNS varchar(50)
AS
  BEGIN
    DECLARE @ParameterValue                 varchar(1024)

    SELECT @ParameterValue = ParameterValue
    FROM dbo.Parameters (NOLOCK)
    WHERE ParameterName = @ParameterName

    RETURN @ParameterValue
  END