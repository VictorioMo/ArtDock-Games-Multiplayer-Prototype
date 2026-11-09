# ARTDOCK — Multiplayer Physics Prototype

A small cooperative multiplayer prototype developed in **Unity 6** as part of the ARTDOCK Network Unity C# Developer technical assignment.

The project supports up to **4 players** in a host-client session, featuring synchronized player movement, networked scene loading, and interactive physics objects.

## 1. Getting Started

### Requirements

- Unity 6.0 or newer
- Netcode for GameObjects
- Unity Transport
- Unity Input System

### Running the Project

1. Clone the repository.
2. Open the project using Unity Hub.
3. Open the `Launch` scene.
4. Enter Play Mode.

The `Launch` scene initializes the persistent `NetworkManager` and automatically loads the `Bootstrap` scene containing the connection menu.

### Hosting a Session

1. Select **Host** from the main menu.
2. Configure the connection port if necessary.
3. Other players can join using the host's IP address and port.
4. Once connected, players appear in the lobby.
5. The host presses **Start** to load the gameplay scene.

For testing multiple instances on the same computer, clients can connect using `127.0.0.1`.

For connections between different computers, use the host's LAN IP address or a reachable public IP with the appropriate UDP port configuration.

No command-line arguments are currently required or implemented.

### Controls

| Input | Action |
|---|---|
| WASD | Movement |
| Mouse | Camera rotation |
| B | Spawn a physics ball |
| E | Grab / Drop |
| F | Throw held ball |
| X | Delete held ball |

## 2. Architecture

The project uses a lightweight, modular architecture, separating session management, UI, networking configuration, and gameplay responsibilities.

### Main Components

**Session Management**

- `SessionInstaller` — Composition root responsible for initializing services and injecting dependencies.
- `INetworkSessionService` — Interface exposing session-related operations.
- `NgoNetworkSessionService` — Netcode for GameObjects implementation of the session interface.
- `ConnectionApprovalService` — Handles incoming connection approval and player limits.
- `SessionExitHandler` — Handles session shutdown and returning to the main menu.

**Lobby & Scene Management**

- `LobbyState` — Maintains the synchronized list of connected players.
- `GameSessionController` — Controls the transition between lobby and gameplay, including network scene synchronization.
- `ConnectionUI` / `LobbyUI` — User interfaces for connection and lobby interactions.

**Gameplay**

- `PlayerMovement` — Handles player movement, camera rotation, and movement activation.
- `PlayerBallSpawner` — Requests server-side physics object spawning.
- `PlayerBallInteractor` — Handles player interaction requests through RPCs.
- `NetworkBall` — Manages physics object ownership, holding, dropping, throwing, and disconnect recovery.

### Dependencies

```text
                    SessionInstaller
                           |
                 INetworkSessionService
                           |
                NgoNetworkSessionService
                           |
                  Unity Transport


                 GameSessionController
                           |
                  NetworkSceneManager
                           |
                    Gameplay Scene


                   PlayerBallInteractor
                           |
                    Server-side RPCs
                           |
                      NetworkBall
                           |
                NetworkTransform / Rigidbody
```

Dependency injection is implemented through a custom composition root rather than an external DI framework.

Session operations are exposed through interfaces, allowing the transport/session implementation to be replaced without rewriting the UI logic.

Gameplay currently uses NGO networking components directly, but it does not depend on a specific underlying transport.

### Extensibility

**Adding another transport:** Implement the necessary session adapter, update its registration in `SessionInstaller`, and configure the corresponding transport in `NetworkManager`.

**Adding another physics object:** Create a networked prefab with the required physics and synchronization components. The current interaction system specifically targets `NetworkBall`; supporting multiple object types would benefit from extracting a shared interactable interface.

## 3. Networking Decisions

### Network Topology

The project uses a **listen-server architecture**.

The host is responsible for:

- Session management
- Connection approval
- Network scene transitions
- Physics object spawning and destruction
- Object interaction validation
- Physics simulation

Clients send interaction requests to the host, which validates and executes them.

### Player Synchronization

Player movement uses an **owner-authoritative** approach.

Each player controls their own movement locally, while `NetworkTransform` synchronizes their position and rotation across the network.

This approach provides responsive controls without requiring client-side movement prediction and reconciliation.

Remote player movement uses network interpolation to reduce visible jitter.

### Physics Synchronization

Physics objects use **server-authoritative simulation**.

The host handles gravity, collisions, and applied forces. Other clients receive synchronized object transforms through NGO.

Non-authoritative Rigidbody instances are configured as kinematic, preventing clients from independently simulating gravity and producing conflicting physics results.

This ensures that physics interactions are primarily determined by the host.

### Object Spawning

Players request object creation using a server RPC.

The host instantiates and spawns the object through `NetworkObject.Spawn()`.

Newly created objects belong to the server, not the player who requested them. Consequently, they remain in the session when their creator disconnects.

### Grabbing, Dropping & Throwing

Object interactions follow a server-validated request model.

When grabbing an object:

1. The player identifies an object using a camera raycast.
2. The client sends its `NetworkObjectId` to the server.
3. The server verifies the requesting player, interaction distance, and object availability.
4. If accepted, the server assigns the object to the requesting player.
5. Network ownership is transferred to the holder.
6. The server keeps the object at a holding position relative to the player.

While held, the object is kinematic on the server.

When dropped or thrown:

1. The server verifies the current holder.
2. Ownership returns to the host.
3. Dynamic physics are restored.
4. Throwing applies an impulse on the server.

The server processes interaction requests and maintains the holding state, preventing multiple players from successfully grabbing the same object simultaneously.

The current holder is synchronized using a `NetworkVariable`.

### Connection Lifecycle

Connection approval restricts the session to a maximum of four players.

The lobby maintains a synchronized player list using `NetworkList`.

Gameplay is loaded through NGO's network scene management system. Player movement is activated after the coordinated loading phase completes.

**Late joining** is supported: new players synchronize with the ongoing gameplay scene and receive existing spawned objects and their current network state.

**Client disconnections** are handled without terminating the remaining session. Objects held by disconnected players are released and remain available.

**Host disconnections** terminate the session and return connected clients to the main menu.

Host migration is not implemented.

### Network Traffic

The project primarily uses NGO's built-in synchronization mechanisms:

- `NetworkTransform` for continuous spatial updates.
- `NetworkVariable` and `NetworkList` for persistent synchronized state.
- RPCs for discrete player actions such as spawning, grabbing, dropping, throwing, and deleting objects.

This avoids manually transmitting object positions through custom RPCs every frame.

## 4. Testing & Known Limitations

### Testing

The prototype has been tested using multiple local instances.

The following functionality was verified during development:

- Host/client connections
- Lobby synchronization
- Network scene transitions
- Late joining
- Object spawning
- Grabbing, dropping, throwing, and deleting objects
- Object ownership transfers
- Client and host disconnections
- Returning to the main menu after session termination

Network simulation was also used during development to evaluate behavior under approximately **150 ms RTT** and packet loss.

### Known Limitations

**Held-object movement:** Player movement is owner-authoritative, while object physics are server-authoritative. This can introduce visible delay or minor interference with player movement when clients carry physics objects.

A possible improvement would be local visual prediction of held objects while preserving server authority over their actual physics state.

**Physics synchronization:** Under significant network latency or packet loss, physics objects may exhibit visible corrections due to delayed network updates.

**Scene loading interruptions:** Client disconnection during an active scene-loading transition requires additional verification.

**Connection approval:** The current implementation enforces the maximum player count but does not provide all optional validation features, such as persistent player identity or build-version checks.

**Player identification:** The lobby currently uses network client identifiers rather than persistent player names.

**Transport:** Unity Transport with direct IP/port connections is implemented. Steam P2P, Steam lobbies, and related integrations are outside the current scope.

### Future Improvements

- Client-side visual prediction for held objects
- More robust handling and testing of interrupted scene transitions
- Improved feedback for rejected connection and interaction requests
- Additional physics object types through a common interaction interface
- Advanced connection validation and player identification

## 5. Development Time

Recorded development time:

| Day | Hours |
|---|---:|
| Wednesday | 3 |
| Thursday | 4 |
| Friday | 1 |
| Sunday | 3 |
| Monday | 3 |
| **Total** | **14** |

The estimate includes project setup, implementation, networking tests, debugging, and documentation.

---

**Developer:** Victorio Moraru

**Technical Assignment:** ARTDOCK — Network Unity C# Developer

**Repository:** https://github.com/VictorioMo/ArtDock-Games-Multiplayer-Prototype