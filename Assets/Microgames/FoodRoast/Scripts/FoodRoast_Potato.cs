﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FoodRoast { 
  public enum PotatoCookedState { Uncooked, Cooked, Burnt}

  [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
  public class FoodRoast_Potato : MonoBehaviour {
    private SpriteRenderer _SpriteRenderer;
    public SpriteRenderer SpriteRenderer {
      get {
        if (_SpriteRenderer == null)
          _SpriteRenderer = GetComponent<SpriteRenderer>();
        return _SpriteRenderer;
      }
    }

    private ParticleSystem _Particles;
    public ParticleSystem Particles {
      get {
        if (_Particles == null)
          _Particles = GetComponentInChildren<ParticleSystem>();
        return _Particles;
      }
    }

    [System.Serializable]
    private struct PotatoSpritePack {
      public Sprite[] Pack;
    }

    [Header("Potato Sprites (0 - Uncooked, 1 - Cooked, 2 - Burnt)")]
    [SerializeField] private PotatoSpritePack[] PotatoSprites;
    private Sprite GetSprite => PotatoSprites[(int)PotatoCookedState].Pack[PotatoAnimationState];
    private int GetPackLength => PotatoSprites[0].Pack.Length;

    [Header("Animation States")]
    [SerializeField] private float StateDuration = 0.33f;
    [SerializeField] private int PotatoAnimationState = 0;
    [ReadOnly] [SerializeField] private PotatoCookedState PotatoCookedState = 0;

    private void Start() {
      PotatoCookedState = PotatoCookedState.Uncooked;
      StartCoroutine(AnimationCoroutine());
      StartCoroutine(CookingCoroutine());
    }

    private IEnumerator AnimationCoroutine(){
      float time = 0;
      while (true) {
        SpriteRenderer.sprite = GetSprite;
        while (time < StateDuration) {
          yield return null;
          time += Time.deltaTime;
        }
        time -= StateDuration;
        PotatoAnimationState = (PotatoAnimationState + 1) % GetPackLength;
      }
    }

    // Timer till cooked potatoes
    private IEnumerator CookingCoroutine() {
      yield return new WaitForSeconds(FoodRoast_Controller.Instance.GetCookTime());
      CookedProcedure();
      yield return new WaitForSeconds(FoodRoast_Controller.Instance.GetPotatoBurnTime);
      BurntProcedure();
    }

    // Procedure that turns uncooked to cooked potatoes
    private void CookedProcedure() {
      PotatoCookedState = PotatoCookedState.Cooked;
      SpriteRenderer.sprite = GetSprite;

      ChangeParticlesStartColorAlpha(.05f);
      Particles.Play();
      EmitParticles(4);
    }

    // Procedure that turns uncooked to cooked potatoes
    private void BurntProcedure() {
      PotatoCookedState = PotatoCookedState.Burnt;
      SpriteRenderer.sprite = GetSprite;

      ChangeParticlesStartColorAlpha(.4f);
      Particles.Play();
      EmitParticles(4);

      FoodRoast_Controller.Instance.AddUncookedPotato();
    }

    private void EmitParticles(int amount){
      var shape = Particles.shape;
      var tempArc = shape.arcMode;
      shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
      Particles.Emit(amount);
      shape.arcMode = tempArc;
    }

    private void ChangeParticlesStartColorAlpha(float alpha){
      var main = Particles.main;
      var colorStruct = main.startColor;
      var color = colorStruct.color;

      color.a = alpha;
      colorStruct.color = color;
      main.startColor = colorStruct;
    }

    // Procedure when the potato is clicked
    private void OnMouseDown() {
      if (PotatoCookedState == PotatoCookedState.Cooked) {
        FoodRoast_Controller.Instance.AddCookedPotato();
      } else if (PotatoCookedState == PotatoCookedState.Uncooked) {
        FoodRoast_Controller.Instance.AddUncookedPotato();
      }
      Particles.Stop();
      Particles.transform.SetParent(null);

      Destroy(gameObject);
    }
  }
}
