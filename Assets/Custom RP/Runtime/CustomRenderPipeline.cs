﻿using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline {

	CameraRenderer renderer = new CameraRenderer();
	ShadowSettings shadowSettings = new ShadowSettings();
	bool useDynamicBatching, useGPUInstancing;

	public CustomRenderPipeline (
		bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher,ShadowSettings shadowSettings
	) {
		this.useDynamicBatching = useDynamicBatching;
		this.useGPUInstancing = useGPUInstancing;
		this.shadowSettings = shadowSettings;
		GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
		GraphicsSettings.lightsUseLinearIntensity = true;
	}

	protected override void Render (
		ScriptableRenderContext context, Camera[] cameras
	) {
		foreach (Camera camera in cameras) {
			renderer.Render(
				context, camera, useDynamicBatching, useGPUInstancing,shadowSettings
			);
		}
	}
}