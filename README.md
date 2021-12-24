## fluke

offline-first decentralized run-anywhere e2e encrypted [P.A.R.A.](https://fortelabs.co/blog/para/) implementation

this is my second project using a functional language, where i tried to narrow the knowledge gap from the first one (generative art app),
which lacked proper type modeling and was excessively IO heavy, leveraging only the language ergonomics including default immutability, currying, etc.

P.A.R.A. creator recommends using the technique with your tool of choice (Notion, Obsidian, OneNote, Emacs Org-mode, Evernote, etc), but since the domain
is simple, it looked like a good opportunity to build a foundation without being overwhelmed with business logic, to be reused later for applications
of enterprise size. it was first prototyped using excel.

base domain:

```fs
type Information =
    | Project of project: Project
    | Area of area: Area
    | Resource of resource: Resource
and Project = { Name: ProjectName; Area: Area }
and ProjectName = ProjectName of name: string
and Area = { Name: AreaName }
and AreaName = AreaName of name: string
and Resource = { Name: ResourceName }
and ResourceName = ResourceName of name: string
```

(ps: the idea arose from the type below - which looks very cool -, but soon after I realized that the archive option should be a property instead)

```fs
type Information =
    | Project of project: Project
    | Area of area: Area
    | Resource of resource: Resource
    | Archive of information: Information
```

more about P.A.R.A in [this](https://www.youtube.com/watch?v=yIc5SpuvmJg) podcast episode
