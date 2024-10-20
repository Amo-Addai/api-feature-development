package com.sample.payroll;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.List;
import java.util.stream.Collectors;

import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

import org.springframework.hateoas.Resource;

@RestController
class EmployeeController {

	private final EmployeeRepository repository;

	private final EmployeeResourceAssembler assembler;

	EmployeeController(EmployeeRepository repository,
					   EmployeeResourceAssembler assembler) {

		this.repository = repository;
		this.assembler = assembler;
	}

	// Aggregate root

	@GetMapping("/employees")
	List<Employee> all() {
		return repository.findAll();
	}
    // EITHER OF THESE 2 WILL WORK
    @GetMapping("/employees")
    Resources<Resource<Employee>> all() {

        List<Resource<Employee>> employees = repository.findAll().stream()
            .map(assembler::toResource)
            .collect(Collectors.toList());

        return new Resources<>(employees,
            linkTo(methodOn(EmployeeController.class).all()).withSelfRel());
    }

	@PostMapping("/employees")
	Employee newEmployee(@RequestBody Employee newEmployee) {
		return repository.save(newEmployee);
	}
    // EITHER OF THESE 2 WILL WORK
    @PostMapping("/employees")
    ResponseEntity<?> newEmployee(@RequestBody Employee newEmployee) throws URISyntaxException {

        Resource<Employee> resource = assembler.toResource(repository.save(newEmployee));

        return ResponseEntity
            .created(new URI(resource.getId().expand().getHref()))
            .body(resource);
    }

	// Single item

	@GetMapping("/employees/{id}")
	Employee one(@PathVariable Long id) {

		return repository.findById(id)
			.orElseThrow(() -> new EmployeeNotFoundException(id));
	}
    // EITHER OF THESE 2 WILL WORK
    @GetMapping("/employees/{id}")
    Resource<Employee> one(@PathVariable Long id) {

        Employee employee = repository.findById(id)
            .orElseThrow(() -> new EmployeeNotFoundException(id));

        return assembler.toResource(employee);
    }

	@PutMapping("/employees/{id}")
	Employee replaceEmployee(@RequestBody Employee newEmployee, @PathVariable Long id) {

		return repository.findById(id)
			.map(employee -> {
				employee.setName(newEmployee.getName());
				employee.setRole(newEmployee.getRole());
				return repository.save(employee);
			})
			.orElseGet(() -> {
				newEmployee.setId(id);
				return repository.save(newEmployee);
			});
	}
    // EITHER OF THESE 2 WILL WORK
    @PutMapping("/employees/{id}")
    ResponseEntity<?> replaceEmployee(@RequestBody Employee newEmployee, @PathVariable Long id) throws URISyntaxException {

        Employee updatedEmployee = repository.findById(id)
            .map(employee -> {
                employee.setName(newEmployee.getName());
                employee.setRole(newEmployee.getRole());
                return repository.save(employee);
            })
            .orElseGet(() -> {
                newEmployee.setId(id);
                return repository.save(newEmployee);
            });

        Resource<Employee> resource = assembler.toResource(updatedEmployee);

        return ResponseEntity
            .created(new URI(resource.getId().expand().getHref()))
            .body(resource);
    }

	@DeleteMapping("/employees/{id}")
	void deleteEmployee(@PathVariable Long id) {
		repository.deleteById(id);
	}
    // EITHER OF THESE 2 WILL WORK
    @DeleteMapping("/employees/{id}")
    ResponseEntity<?> deleteEmployee(@PathVariable Long id) {

        repository.deleteById(id);

        return ResponseEntity.noContent().build();
    }
}