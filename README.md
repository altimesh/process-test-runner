# process-test-runner
A unit test runner in which tests run in separate processes


## Why 
Sometimes, you want to make sure your tests run in complete isolation.
Sometimes, you want to test stuff that can crash its process with abort exit(x). 

In thoses cases, most of UT framework will completely crash and you can't have a full report, if a report (result dumps happens after all tests run). 

## What it does
It's mainly written for internal purposes but : 
- it discovers and runs tests (nunit or mstest)
- each test run in its own process
- at some extend, setup and teardown are supported as well as sequences
- it's fast

## What it does not
It's definitely not (yet) a full agnostic test runner. It lacks stability and many features. It lacks an adapter for IDE's and a way to debug. 
