import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditableTextBlockComponent } from './editable-text-block.component';
import { FormsModule } from '@angular/forms';
import { QuillModule } from 'ngx-quill';

describe('EditableTextBlockComponent', () => {
  let component: EditableTextBlockComponent;
  let fixture: ComponentFixture<EditableTextBlockComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormsModule, QuillModule.forRoot(), EditableTextBlockComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(EditableTextBlockComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
