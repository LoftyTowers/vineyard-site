import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditableImageBlockComponent } from './editable-image-block.component';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

describe('EditableImageBlockComponent', () => {
  let component: EditableImageBlockComponent;
  let fixture: ComponentFixture<EditableImageBlockComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommonModule, FormsModule, EditableImageBlockComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(EditableImageBlockComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
