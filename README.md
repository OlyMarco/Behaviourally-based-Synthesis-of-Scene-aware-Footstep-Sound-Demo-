# Behaviourally-based Synthesis of Scene-aware Footstep Sound

![Screenshot](./images/main.png)

> **A real-time procedural audio synthesis system that generates context-aware footstep sounds based on character behavior states and environmental surface materials in Unity3D.**

## 🎯 Overview

This Unity3D demo implements an advanced **behaviorally-driven footstep audio synthesis engine** that dynamically generates realistic footstep sounds by analyzing:

- **Character Locomotion States**: Walking, running, crouching, jumping with distinct audio profiles
- **Surface Material Detection**: Real-time terrain and mesh material identification via raycast-based texture sampling
- **Environmental Acoustics**: Indoor/outdoor spatial audio processing with reverb and pitch modulation
- **Physical Interaction Audio**: Height-based landing sounds and velocity-triggered step audio

### 🔬 Technical Innovation

- **Multi-modal Material Detection**: Unified system supporting both Unity Terrain splatmaps and mesh-based material identification
- **Behavioral Audio Parameterization**: Dynamic volume, pitch, and frequency modulation based on movement patterns
- **Spatial Audio Synthesis**: 3D positioned audio sources with distance-based linear attenuation
- **Real-time Acoustic Environment Classification**: Automatic indoor/outdoor detection affecting reverb characteristics

## 📊 System Architecture

### Core Components

| Component | File | Lines | Primary Function |
|-----------|------|-------|-----------------|
| **Audio Synthesis Engine** | `PlayerSynthesis.cs` | 463 | Footstep generation, material detection, behavior analysis |
| **Environmental Audio Manager** | `EnvSetting.cs` | 120 | Ambient sound management, 3D spatial audio |
| **Debug & Utilities** | `BugRepairer.cs` | 206 | Material visualization, scene navigation, environment detection |

### Audio Processing Pipeline

```
Character Input → Behavior Analysis → Surface Detection → Audio Parameter Calculation → 3D Audio Rendering
     ↓               ↓                    ↓                       ↓                        ↓
  WASD+Mouse    Walk/Run/Crouch    Raycast+Texture       Volume/Pitch/Frequency      Spatial Audio
```

## ⚙️ Technical Parameters

### Behavior-Audio Mapping

| Locomotion State | Volume Range | Pitch Range (Outdoor) | Pitch Range (Indoor) | Step Distance Trigger |
|------------------|--------------|----------------------|---------------------|---------------------|
| **Walking** | 0.4 - 0.6 | 0.88 - 1.12 | 0.45 - 0.55 | 1.0m |
| **Running** | 0.6 - 0.9 | 1.35 - 1.85 | 0.65 - 0.72 | 0.92m |
| **Crouching** | 0.3 - 0.5 | 0.62 - 0.68 | 0.35 - 0.47 | 0.35m |
| **Landing** | 0.4 + (1.3 × height) | Variable | Variable | Height-based trigger |

### Material Detection System

- **Terrain Support**: Automatic splatmap sampling with dominant texture identification
- **Mesh Support**: Triangle-level material detection via raycast intersection
- **Surface Library**: Extensible material-to-audio mapping system
- **Performance**: Single raycast per frame with cached results

### Environmental Acoustics

| Environment | Volume Multiplier | Pitch Modifier | Reverb Characteristics |
|-------------|------------------|----------------|----------------------|
| **Outdoor** | 1.0× | 1.1× | Clear, direct sound |
| **Indoor** | 0.5× | 0.9× | Muffled, reverberant |

## 🚀 Installation & Setup

### Prerequisites
- **Unity Engine**: 2021.3.16f1c1 or compatible LTS version
- **Render Pipeline**: Universal Render Pipeline (URP)
- **Platform**: Windows/Mac/Linux standalone builds supported

### Installation Steps

1. **Download Project**: [Complete Unity Project](https://drive.google.com/file/d/1cGjgegC9f3vMLgW8L4dqG2sMCScL_O1r/view?usp=share_link)

2. **Import Dependencies**: [Viking Village URP](https://assetstore.unity.com/packages/essentials/tutorial-projects/viking-village-urp-29140)
   
3. **Scene Setup**:
   - Open `DemoScene/The_Viking_Village.unity`
   - Ensure URP renderer settings are active
   - Verify Player prefab configuration in scene

### Controls & Debug Interface

| Category | Key | Function | Technical Details |
|----------|-----|----------|------------------|
| **Locomotion** | WASD + Mouse | Character movement | 6DOF movement with physics-based control |
| | Left Shift | Sprint mode | 1.8× speed multiplier, enhanced FOV |
| | Left Control | Crouch | 0.33× speed, height reduction to 40% |
| | Space | Jump | Base velocity 5.0 + extra boost 1.0 |
| **Debug** | Left Alt | Material display | Real-time texture name overlay |
| | Numpad 1-6 | Scene teleportation | Predefined debug positions |
| | N / M | Height adjustment | Vertical offset for teleport points 5-6 |
| | Escape | Application exit | Immediate termination |

## 🔊 Audio Asset Structure

```
Audio/
├── Grass Terrain/          # 9 grass footstep variants
│   └── Footsteps_Grass_Tall_Movement_[01-09].wav
├── Wood Floor/             # 10 wooden surface variants  
│   └── Footsteps_Wood_Run_[01-10].wav
└── Env/                    # Environmental ambience
    ├── Fire.wav            # Fireplace audio
    └── Wind.mp3            # Wind ambience
```

### Audio Technical Specifications
- **Format**: WAV (uncompressed) / MP3 (compressed ambient)
- **Spatial Audio**: 3D positioned sources with linear rolloff
- **Max Distance**: 10 meters (footsteps), variable (environment)
- **Anti-repetition**: Sequential sample exclusion algorithm

## 📚 Research Context

This implementation demonstrates the concepts presented in:

**"Behaviourally-based Synthesis of Scene-aware Footstep Sound"**  
*IEEE Publication*: [10108832](https://ieeexplore.ieee.org/document/10108832/)

### Key Research Contributions
- Real-time behavioral audio synthesis methodology
- Surface-aware procedural sound generation
- Environmental acoustic modeling for interactive media

## 🎥 Demonstration

**Live Demo Video**: [YouTube - Scene-aware Footstep Synthesis](https://youtu.be/iDY0xn7eX9Y)

## 🏗️ Implementation Details

### Material Detection Algorithm
```csharp
// Unified surface detection supporting Terrain and Mesh colliders
int GetSurfaceIndex(Collider collider, Vector3 worldPosition)
{
    if (collider is TerrainCollider)
        return GetTerrainDominantTexture(worldPosition);
    else 
        return GetMeshMaterialIndex(worldPosition);
}
```

### Behavioral Audio State Machine
- **State Transitions**: Smooth interpolation between locomotion modes
- **Parameter Interpolation**: Lerp-based volume and pitch transitions
- **Performance Optimization**: Distance-based triggering vs. fixed timesteps

---

*Built with Unity 2021.3.16f1c1 • Universal Render Pipeline • Viking Village URP Asset Package*
