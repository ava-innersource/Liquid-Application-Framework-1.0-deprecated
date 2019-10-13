# How to contribute

One of the easiest ways to contribute is to participate in discussions on GitHub issues. You can also contribute by submitting pull requests with code changes.

## General feedback and discussions?
Start a discussion on the [repository issue tracker](https://github.com/Avanade/Liquid-Application-Framework/issues).

## Bugs and feature requests?
For non-security related bugs, log a new issue in the GitHub repository. 

## Reporting security issues and bugs
Security issues and bugs should be reported privately, via email, to a repository admin. You should receive a response within 24 hours. 

## Contributing code and content

We accept fixes and features! Here are some resources to help you get started on how to contribute code or new content.

* ["Help wanted" issues](https://github.com/Avanade/Liquid-Application-Framework/labels/help%20wanted) - these issues are up for grabs. Comment on an issue if you want to create a fix.
* ["Good first issue" issues](https://github.com/Avanade/Liquid-Application-Framework/labels/good%20first%20issue) - we think these are a good for newcomers.

### Identifying the scale

If you would like to contribute to one of our repositories, first identify the scale of what you would like to contribute. If it is small (grammar/spelling or a bug fix) feel free to start working on a fix. If you are submitting a feature or substantial code contribution, please discuss it with the team and ensure it follows the product roadmap. You might also read these two blogs posts on contributing code: [Open Source Contribution Etiquette](http://tirania.org/blog/archive/2010/Dec-31.html) by Miguel de Icaza and [Don't "Push" Your Pull Requests](https://www.igvita.com/2011/12/19/dont-push-your-pull-requests/) by Ilya Grigorik. All code submissions will be rigorously reviewed and tested by the team, and only those that meet an extremely high bar for both quality and design/roadmap appropriateness will be merged into the source.

### Submitting a pull request

If you don't know what a pull request is read this article: https://help.github.com/articles/using-pull-requests. **Make sure the repository can build and all tests pass.** Familiarize yourself with the project workflow and our coding conventions. The coding, style, and general engineering guidelines are below on the topic [Engineering guidelines](#Engineering-guidelines).

### Feedback

Your pull request will now go through **extensive checks** by the subject matter experts on our team. Please be patient. Update your pull request according to feedback until it is approved by one of the ASP.NET team members. After that, one of our team members may adjust the branch you merge into based on the expected release schedule.

# Engineering Guidelines

## General

We try to hold our code to the higher standards. Every pull request must and will be scrutinized in order to maintain our standard. Every pull request should improve on quality, therefore any PR that decreases the quality will not be approved.

We follow coding best practices to make sure the codebase is clean and newcomers and seniors alike will understand the code. This document provides a guideline that should make most of our practices clear, but gray areas may arise and we might make a judgment call on your code, so, when in doubt, question ahead. Issues are a wonderful tool that should be used by every contributor to help us drive the project.

All the rules here are mandatory; however, we do not claim to hold all the answers - you can raise a question over any rule any time (through issues) and we'll discuss it. 

Finally, this is a new project and we are still learning how to work on a Open Source project. Please bear with us while we learn how to do it best.

## Code Style

Code style is usually enforced by Analyzers; any change to those rules must be discussed with the team before it's made. Also, any pull request that changes analyzers rules and commits code will be reproved immediately.

## Good practices

The items below point out the good practices that all code should follow.

### Zero warnings

Compiler warnings should usually be dealt with by correcting the code. Only discussed warnings may be allowed to be marked as exceptions.

### Inner documentation

All public members must be documented. Documentation should clarify the purpose and usage of code elements, so comments such as FooManager: "manages foo" will be rejected. Classes that implement interface may use comment inheritance `/// <inheritdoc/>`, but use it sparingly. 

Try to use examples and such in classes to enable users to understand them more easily. 

If you don't believe that a class or method deserves to be documents, ask yourself if it can be marked as non-public. 

If should comment every non-public class or member that is complex enough. 

All comments should be read-proof.

## Tests

- Tests need to be provided for every bug/feature that is completed.
- Tests only need to be present for issues that need to be verified by QA (for example, not tasks)
- Pull requests must strive to not reduce code coverage.
- If there is a scenario that is far too hard to test there does not need to be a test for it.
  - "Too hard" is determined by the team as a whole.
