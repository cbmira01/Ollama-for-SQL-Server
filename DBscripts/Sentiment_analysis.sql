Use Test;
GO

CREATE TABLE support_emails (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email_content NVARCHAR(2000),
    sentiment NVARCHAR(20),
    sentiment_level INT
);
GO

INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The new washing machine is fantastic! It''s really quiet and efficient.', 'glad', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The oven I bought isn''t heating up properly. Frustrated with the experience.', 'angry', 3);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('My fridge arrived with a scratch on the door. Can this be resolved?', 'neutral', 2);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Thank you for the prompt service with the dryer repair! Appreciated.', 'glad', 3);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('I need assistance setting up my dishwasher. The instructions are unclear.', 'neutral', 1);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The microwave isn''t working, and I''ve only had it a week. Really disappointed.', 'sad', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Very unhappy! The dryer is making a loud noise and seems defective.', 'angry', 5);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The vacuum cleaner is performing well. No complaints here!', 'glad', 2);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('I haven''t received my order confirmation. Is there a delay?', 'neutral', 2);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The freezer has been perfect. Thanks for the recommendation!', 'glad', 5);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The stove is not what I expected. Feel let down.', 'sad', 3);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Can someone explain the warranty terms for my washing machine?', 'neutral', 1);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Thanks for helping me with the installation issues. Much appreciated!', 'glad', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The dishwasher is already leaking. Very disappointed with the quality.', 'angry', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('I’m pleased with the service team’s response time. Great job!', 'glad', 3);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Still no resolution on my broken fridge. Very frustrated.', 'angry', 5);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('I''m a bit confused by the setup instructions for the oven.', 'neutral', 2);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('Your customer support really went above and beyond. Thanks!', 'glad', 5);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('The air conditioner broke down after two months. Extremely upset.', 'angry', 4);
INSERT INTO support_emails (email_content, sentiment, sentiment_level) VALUES ('My order arrived promptly. Appreciate the efficient service.', 'glad', 3);
GO

DECLARE @modelName NVARCHAR(MAX) = 'llama3.2';
DECLARE @prompt NVARCHAR(MAX) = 'Provide a single word sentiment label:';
SELECT 
    id,
    email_content,
	sentiment,
    dbo.CompletePrompt(@prompt, email_content) AS sentiment_analysis
FROM 
    support_emails;
GO
