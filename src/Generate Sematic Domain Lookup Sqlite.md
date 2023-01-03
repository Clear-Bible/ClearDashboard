# GenerateSemantiDomainLookupSqlite
This program takes the SDBH/SDBG XML databases found under the 
`ClearDashboard.DAL\Resources` directory and parses out the 
Greek/Hebrew words.  It then normalizes the word, removes any 
punctuation/diacritics, and lowercases the Greek words.

In case of newer SDBH/SDBG versions, just replace the data 
under the `ClearDashboard.DAL\Resources` directory and 
rerun this program.
