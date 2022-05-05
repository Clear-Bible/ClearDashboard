# CQRS Cheat Sheet

This serves as a guide for adding CQRS Feature Slices to the DAL, Paratext plug-in and eventually Server.

## Background

The feature slice or vertical slice architecutre is built on top of the CQRS and the Mediator design pattern.  CQRS stands for "Command Query Responsibility Segregation". This is for separate reads and writes operations in applications. In this pattern read operations are called ‘Queries’ and Write operations are called ‘Commands’. 