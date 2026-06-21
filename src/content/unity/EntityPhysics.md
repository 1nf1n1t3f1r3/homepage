# EntityPhysics

[💻 View Scripts on GitHub](https://github.com/1nf1n1t3f1r3/Physics-Minirepo)

So, rapid-fire, the way you might start a Unity project is by creating some ground to walk on, for which I used a 'Tilemap', a block-based Unity default that basically turns the world into a grid. I created a simple player object (I wound up using a free-to-use Wolf asset called 'Massacre' (he's adorable)) and adding a 'PlayerController' to it so you can start to move it around with WASD and Spacebar. By default, you can add a 'RigidBody' to a GameObject, and that'll make it affected by gravity. However... The problem with RigidBodies are that Unity physics are non-determinsitic, leading to potential issues with it functioning differently when doing Multiplayer. And, of course, I wanted Multiplayer. That's where creating a custom physics solution to replace it comes in.

So, before we continue, the script turned into a bit of a behemoth over time. The HandlePhysics() function is basically a summary of itself. But, I'll start at the beginning. Also, the EntityRaycastSystem is a part of it, of course. It can be found in the Github link, but I won't go into too much detail on it here just to keep things a little more brief.

## Variables

The key values are Position and Velocity. Position is determined via transform.position, which is the Unity default. However, having multiple things impact transform.position simultaneously doesn't work so well, so everything goes through the Getter/Setters Position and Velocity, where Position gets updated every tick (not 'Update' or 'FixedUpdate', because we're designing with Multiplayer in mind...) based on the Velocity it possesses.

Next thing you might notice are 8 blocks about what it means to be standing on the ground; it's almost philosophical. 'GroundedDown' is the default, of course, and that started just based on being in contact with any piece of 'Ground'. Then you start to think that, maybe, hanging from the ceiling like Spiderman shouldn't count, which is where directions come in.The 'Slope' logic is interesting because, even if the Tilemap is based on Blocks, it can still easily feature 45 degree slopes, or whatever kind of degree slope one feels like making. Whatever decision comes out of 'What actually is a slope and what isn't' is always going to be a little arbitrary, but it's interesting to play around with. Ignoring them because the game only uses blocks is just lazy, right?

## HandlePhysics Update Loop

The way any entity gets updated is through here. There's three main systems: InternalForces, ExternalForces and EntityRaycastSystem. I'll describe them before we continue.

### Internal Forces

This includes things like Gravity, so things fall down, and Drag, so nothing slides like it's on ice. It uses the Grounded (an WallSliding) checks to see if we're in the air or not, then uses the targetFallSpeed and the gravity setting to speed up the velocity.y to send us back to Earth. The velocity.x is determined by Drag/Friction. They're essentially the same, but friction is greater than drag, like how sliding across the ground has more resistance than flying through it.

<!-- Gif Here: Gravity -->

### External Forces

External Forces interact a bit differently. There's three ways it adds Velocity. Either immediately, over time, or it can completely override Velocity (either X or Y, or both!). The logic here is very minimal, but the good part about it is that this only processes the forces. It doesn't care where they come from. In fact, it also includes the Player's inputs from PlayerController (which really wouldn't be classified as something 'External'). The other script will simply add some force to the Entity, and this will process it, no questions asked.

### EntityRaycastSystem

This sends out Raycasts and interprets the results. Raycasts are basically lines that are attached to the Entity at set distances alongside its boundary, scouting ahead in the direction the entity is traveling to see if it's about to collide with anything. The main point of the Raycasts is to prevent 'Tunnelling', which means moving so fast that hitting a surface doesn't stop the entity _before_ it moves into the surface, but _after_. In other words, it'll be stuck inside. Getting this right is mostly a lot of value-tweaking. What I like about is that it has a bit of a feedback loop, where the length of the Raycasts gets increased depending on the Velocity. Further, the diagonal rays also change their angle depending on the Entity's Velocity. If there's a lot of Velocity.X, it'll be angled more to the Left/Right. if there's a lot of Velocity.Y, it'll angle more Upward/Downward. Additionally, it can handle entities of various sizes based on its 'rayCount'. If an entity doesn't have enough Raycasts, there's a risk the rays will miss a piece of ground, because it somehow slips right between the Raycasts it has. For performance reasons, it's best to use as few/short Raycasts as possible.

Another thing that it does is 'snap' to the ground. The idea here is that it's essentially teleports the entity to the ground, to prevent the tunnelling. I'm still not 100% sure on how I feel about it. If not done right, it feels very jittery. But, I also couldn't come up with any better way to do it. What's quite interesting about seeing it in action is that, this 'Snapping' actually accounts for a lot of the movement by itself, at least when moving across diagonal surfaces. It's something I hadn't really thought about before seeing the console logs. The Snapping that's actually even visible, though, is quite rare.

As an additional safeguard, there's 'Repel' logic. This basically checks, based on a box around the Entity, if it's in a wall. The way it works is that WouldOverlapAtPosition checks 'Would I be in a wall here?', before moving there. If so, it'll start to look for the 'exit' with BestRepelDirection and move towards it with RepelFromOverlap before finally executing the movement. It works iteratively, repelling until it's done, or its max has been reached.

<!-- Gif Here: Raycast Gizmos -->

### HandleImpact

This is the 'Cratering Simulator', but also where slopes get a little funky. I figured that landing on a flat surface or on a sloped surface shouldn't have the same effect, so, here, the Raycasts look at the angle of the ground we're landing on. The idea is that, sliding along an even surface wouldn't hurt, or create a dead-stop, but slamming into a wall would. That's very black and white; measuring velocity and impact angles handles everything that's grey. It allows deflecting Velocities across the slope of a surface. One other thing it does is that, when moving horizontally into an upward slope, the Velocity conversion would lead to positive Y (I.E. going up). That makes sense when bouncing off, but without special handling, it means an entity can just 'climb' by moving into a surface. Setting a cooldown on it was a good fix, though one that feels very hacky.

<!-- Gif Here: Bouncy Slope -->

### The Rest

After all the ground work, this is basically the part that executes. First UpdatePosition, which changes Position based on the Entity's Velocity. There's a call to the RaycastSystem to check for Overlapping. If so, it starts running the tunnelling prevention logic (again). Finally, it damp small velocities to zero, because there's no point in having Velocity.X = .000000000125. Finally, it runs HandleSteepImpactTimer, which handles the cooldown I mentioned in the 'handleImpact' section.

## Conclusion

Looking back on it, the system feels both hacky and surprisingly complete. There are obvious things I'd refactor today: splitting responsibilities into smaller components, cleaning up commented code, and simplifying some of the state synchronization. At the same time, many of the solutions emerged from solving real edge cases. If I built another 2D platformer tomorrow, I'd probably start by reusing large parts of this controller rather than starting from scratch. In my testing, the floatiness of it reminded me a lot of Risk of Rain 2, a game I like quite a lot. Mostly by accident, of course, but I still take it as a good sign.

<!-- Gif Here. Not being able to walk Diagonals -->

<!-- Gif Here Walking Diagonals -->
