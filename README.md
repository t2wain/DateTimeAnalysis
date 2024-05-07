## Analyzing Software License Usages

Given a set of software licenses hosted on several network servers that are available to be shared with users globally, there is a need to know if the quantity of licenses are adequate for users to use the software to perform their works. Example of such information are as follow. 

Current Usages:

- Who are currently using the licenses?
- Are there any licenses currently available?
- How long are the current usage sessions?
- Are some users forgetting to close their software based on the extended duration of their current sessions?
- Are some users using multiple licenses concurrently?

Past Usages:

- How many users using the software?
- How many are active users or occasional user?
- Which locations and time zones are users from?
- When are all licenses currently being used?

How users can use these information:

- Users can monitor the availability of licenses.
- Users can contact and negotiate with other current users based on work deadlines.
- Users can identify trends of license availability and schedule their works.

How management can use these information:

- Define the acceptable level of license availability.
- Determine when to purchase additional licenses based on recent usage trends.

## Usage Data Format

To start, usage data must be collected and pre-process into standard format. Typically, usage data often available as either an event or a session. An event can occurred for the following activities:

- when a license is checked-out
- when a license is checked-in
- when a check-out attempt failed perhaps because license is not available

A session conveys a time duration that a license is in use. A session can be derived from event data by correlating the checked-out and the corresponding checked-in events.

## Usage Data Analysis

Please refer to each project README for additional explanation.
