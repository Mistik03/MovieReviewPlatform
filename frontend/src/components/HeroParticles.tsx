import { useMemo, useRef } from 'react'
import { Canvas, useFrame } from '@react-three/fiber'
import * as THREE from 'three'

const PARTICLE_COUNT = 900

/** Dust drifting through a projector beam — slow, warm, additive points. */
function Dust() {
  const pointsRef = useRef<THREE.Points>(null)

  const positions = useMemo(() => {
    const array = new Float32Array(PARTICLE_COUNT * 3)
    for (let i = 0; i < PARTICLE_COUNT; i++) {
      array[i * 3] = (Math.random() - 0.5) * 9
      array[i * 3 + 1] = (Math.random() - 0.5) * 5
      array[i * 3 + 2] = (Math.random() - 0.5) * 4
    }
    return array
  }, [])

  useFrame((state, delta) => {
    const points = pointsRef.current
    if (!points) return
    points.rotation.y += delta * 0.018
    // Gentle parallax toward the pointer; lerp keeps it smooth and jitter-free.
    const targetX = state.pointer.y * 0.06
    const targetY = state.pointer.x * 0.1
    points.rotation.x = THREE.MathUtils.lerp(points.rotation.x, targetX, 0.02)
    points.rotation.z = THREE.MathUtils.lerp(points.rotation.z, targetY * 0.15, 0.02)
  })

  return (
    <points ref={pointsRef}>
      <bufferGeometry>
        <bufferAttribute attach="attributes-position" args={[positions, 3]} />
      </bufferGeometry>
      <pointsMaterial
        size={0.02}
        color="#d9a648"
        transparent
        opacity={0.5}
        sizeAttenuation
        depthWrite={false}
        blending={THREE.AdditiveBlending}
      />
    </points>
  )
}

export default function HeroParticles() {
  return (
    <Canvas
      className="pointer-events-none"
      camera={{ position: [0, 0, 3.2], fov: 55 }}
      dpr={[1, 1.5]}
      gl={{ antialias: false, powerPreference: 'low-power', alpha: true }}
    >
      <Dust />
    </Canvas>
  )
}
