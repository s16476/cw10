Create Procedure PromoteStudents @Studies NVARCHAR(100), @Semester INT
AS
BEGIN

SET XACT_ABORT ON;
BEGIN TRAN

	DECLARE @IdStudies INT = (Select IdStudy from Studies where name = @Studies);
	IF @IdStudies IS NULL
	BEGIN
		--RAISEERROR
		RETURN;
	END;

	DECLARE @IdEnrollment INT = (Select IdEnrollment from Enrollment where IdStudy = @IdStudies and Semester = (@Semester + 1));
	IF @IdEnrollment IS NULL 
	BEGIN
		SET @IdEnrollment = (SELECT TOP 1 IdEnrollment from Enrollment order by IdEnrollment desc) + 1;
		INSERT INTO Enrollment values (@IdEnrollment, (@Semester + 1), @IdStudies, GETDATE());
	END;

	UPDATE Student SET IdEnrollment = @IdEnrollment where IdEnrollment = (Select IdEnrollment from Enrollment where IdStudy = @IdStudies and Semester = @Semester);

	SELECT * from Enrollment where IdEnrollment = @IdEnrollment;

	COMMIT

END;