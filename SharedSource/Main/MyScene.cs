#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaveEngine.Common;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Math;
using WaveEngine.Components.Cameras;
using WaveEngine.Components.Graphics2D;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Resources;
using WaveEngine.Framework.Services;
using WaveEngine.Materials;
#endregion

namespace SpaceWave
{
  public class MyScene : Scene
  {

    private const float cGravitationalConstant = 0.0008f;

    protected override void Start()
    {
      Entity Planet1 = new Entity() { Tag = "CelestialObject" }
       .AddComponent(new Transform3D() { Position = new Vector3(-2, 0, -10) })
       .AddComponent(new SphereMesh())
       .AddComponent(new SphereCollider3D() { })
       .AddComponent(new MaterialComponent() { Material = new StandardMaterial() { AmbientColor = Color.Red, DiffuseColor = Color.Plum } })
       .AddComponent(new MeshRenderer())
       .AddComponent(new RigidBody3D() { Mass = 1, LinearVelocity = new Vector3(0, 0, 0), AngularVelocity = new Vector3(1, 0, 5) });
      ;

      Entity Planet2 = new Entity() { Tag = "CelestialObject" }
       .AddComponent(new Transform3D() { Position = new Vector3(3, 0, -10) })
       .AddComponent(new SphereMesh())
       .AddComponent(new SphereCollider3D() { })
       .AddComponent(new MaterialComponent() { Material = new StandardMaterial() { AmbientColor = Color.Green, DiffuseColor = Color.YellowGreen } })
       .AddComponent(new MeshRenderer())
       .AddComponent(new RigidBody3D()
       {
         Mass = 1099,
         LinearVelocity = new Vector3(0, 0, 0),
         AngularVelocity = new Vector3(1, 0, 5)
       });
      ;

      EntityManager.Add(Planet1);
      EntityManager.Add(Planet2);
    }

    protected override void CreateScene()
    {
      this.Load(WaveContent.Scenes.MyScene);

      PhysicsManager.Simulation3D.Initialize();
      PhysicsManager.Simulation3D.Gravity = Vector3.Zero;
      PhysicsManager.Simulation3D.FixedTimeStep = 0.001f;

      PhysicsManager.Simulation3D.OnPhysicStep += OnPhysicStep;

      // Where the camera is placed on the world
      var position = new Vector3(0, 0, 2.5f);
      // Where the camera is looking at
      var lookAt = Vector3.Zero;
      var camera = new FreeCamera3D("MainCamera", position, lookAt)
      {
        BackgroundColor = Color.CornflowerBlue
      };
      this.EntityManager.Add(camera.Entity);

    }

    private object mPhysicsLock = new object();

    private void OnPhysicStep(object sender, EventArgs e)
    {
      lock (mPhysicsLock)
      {
        var wObjects = EntityManager.FindAllByTag("CelestialObject");

        foreach (Entity wEntity in wObjects)
        {
          Task.Run(() => UpdateForces(wEntity, wObjects.Except(new[] { wEntity })));
        }
      }
    }


    private void UpdateForces(Entity entity, IEnumerable<object> otherEntites)
    {
      
      var wRigidBody = entity.FindComponent<RigidBody3D>();
      var wTransform = entity.FindComponent<Transform3D>();

      if (wRigidBody == null || wTransform == null) return;

      foreach (Entity wOtherObject in otherEntites)
      {

        var wOtherTransform = wOtherObject.FindComponent<Transform3D>();
        var wOtherRigidBody = wOtherObject.FindComponent<RigidBody3D>();

        if (wOtherRigidBody == null || wOtherTransform == null) continue;


        var wDistanceVector = (wOtherTransform.Position - wTransform.Position);
        var wForceDirection = Vector3.Normalize(wDistanceVector);
        var wDistanceSquared = wDistanceVector.LengthSquared();
        var wPhysicsStepSize = PhysicsManager.Simulation3D.FixedTimeStep;


        var wNewAccelerationVector = wForceDirection * cGravitationalConstant * wOtherRigidBody.Mass / wDistanceSquared;

        wRigidBody.LinearVelocity += wNewAccelerationVector;

      }
    }
  }
}
