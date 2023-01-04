# ContinuumCrowds

Continuum Crowds is a real-time crowd model based on continuum dynamics. This work was presented in **Continuum Crowds** (Treuille, 2006).
https://grail.cs.washington.edu/projects/crowd-flows/78-treuille.pdf

This original implementation is derived from the source material.


## Unity Package Manager support /#upm

Add to your project via the Unity Package Manager. 
1. In the Package Manger, select "Add package from Git URL..."
2. Type in 
```
https://github.com/yohash/ContinuumCrowds.git#upm
```

The `upm` branch is maintained us a current subtree via:
```
git subtree split --prefix=Assets/ContinuumCrowds --branch upm
```

## Dependencies

This package has a dependency on another custom package. To allow for automatic installion of dependencies
- [yohash.math](https://github.com/yohash/Math)
- [yohash.priorityqueue](https://github.com/yohash/PriorityQueue)
please first install the [mob-sakai/GitDependencyResolverForUnity](https://github.com/mob-sakai/GitDependencyResolverForUnity).

The git dependency resolver can be installed easily in the Unity package manager with this direct git link:
```
https://github.com/mob-sakai/GitDependencyResolverForUnity.git
```
