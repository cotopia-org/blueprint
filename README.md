# Blueprint document
## Overview
This system is designed as a versatile workflow automation platform that allows users to create and manage automated workflows between various applications, services, and data sources. It empowers users to streamline processes and reduce manual intervention by connecting different tools in a cohesive, automated manner.

## Entities
### 1) Blueprint
A blueprint refers to a space where a set of nodes is located and is triggered by an event. During this process, a series of operations are performed. 
### 2) Node
A node is a fundamental component within the system. It represents an entity with its own data fields and associated scripts that collectively produce a specific output. Each node can have one or more outputs, allowing it to interact with other nodes and contribute to complex workflows. Nodes are designed to encapsulate specific functions or operations, making it easier to build and manage automated processes by connecting and configuring these components.
### 3) Process
Each blueprint, after being triggered by a webhook, pulse, delay, or manually, is converted into a process. Depending on the use case, processes can either remain active for a long time or be terminated quickly after the process is completed.
### 4)Account 
An account refers to a person who can create, manage, and modify blueprints and nodes in the system.

## Script language
In this system, the popular JavaScript language is used to create node code. This approach gives the system the ability to create or modify nodes without needing to restart the service.
