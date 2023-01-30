import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavMeshComponentComponent } from './nav-mesh-component.component';

describe('NavMeshComponentComponent', () => {
  let component: NavMeshComponentComponent;
  let fixture: ComponentFixture<NavMeshComponentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NavMeshComponentComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NavMeshComponentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
